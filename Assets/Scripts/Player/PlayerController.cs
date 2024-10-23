susing System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using UnityEngine.Animations.Rigging;
using UnityEngine.Animations;
using TMPro;


public class PlayerController : MonoBehaviourPunCallbacks
{
    #region VARIABLES
    public Transform viewPoint;
    [SerializeField]
    private float _sensitivity;
    [SerializeField]
    private float _adsSensitivity;
    [SerializeField]
    private float _walkSpeed;
    [SerializeField]
    private float _runSpeed;
    [SerializeField]
    private float _jumpHeight;
    [SerializeField]
    private float _gravityMultiplier;
    [SerializeField]
    private GameObject _hitEffect;
   // [SerializeField]
   // private float _fireRate;
    [SerializeField]
    private float _maxHeat;
   // [SerializeField]
    //private float _heatPerShot;
    [SerializeField]
    private float _coolRate;
    [SerializeField]
    private float _overHeatedCoolRate;
    [SerializeField]
    private Weapon[] _weapons;
    [SerializeField]
    private float _muzzleTime;

    private float _currentMuzzleTime;
    private int _currentWeapon;
    private Vector2 _mouseInput;
    private float _verticalRotation;
    private Vector3 _moveDir,_movement;
    private CharacterController _charCon;
    private Camera _cam;
    private float _canFire;
    private float _currentHeat;
    private bool _isOverHeated;
    public Animator anim;
    public GameObject playerModel;

    private PostProcessVolume effects;
    private ChromaticAberration gotDamaged;

    public Rig leftHandRig;
    public GameObject gunHolder;

    public int maxHealth=100;
    private int _currentHealth;
    public GameObject playerHitImpact;

    public SkinnedMeshRenderer[] playerSkins;
    public AudioSource criticalDamage;

    //recoil
    public float hRecoil=0f;
    public float vRecoil=0f;

    private FixedJoystick _joyStick;

    private Button _shootButton;
    private Button _jumpButton;
    private Button _adsButton;

    #endregion

    void Start()
    {
        UIManager.obj.normalSensitivitySlider.value = PlayerPrefs.GetFloat("sensi");
        UIManager.obj.adsSensitivitySlider.value = PlayerPrefs.GetFloat("ads");
        UIManager.obj.musicSlider.value = PlayerPrefs.GetFloat("music");
        UIManager.obj.sniperScope.SetActive(false);
        Camera.main.fieldOfView = 60f;
        
        playerSkins[photonView.Owner.ActorNumber % playerSkins.Length].gameObject.SetActive(true);

        photonView.RPC("SetGun", RpcTarget.All, _currentWeapon);
        effects = Camera.main.GetComponent<PostProcessVolume>();
        effects.profile.TryGetSettings<ChromaticAberration>(out gotDamaged);

        //respawn
        if (photonView.IsMine)
        {
            _shootButton = UIManager.obj.shootButton;
            _adsButton = UIManager.obj.adsButton;
            _jumpButton = UIManager.obj.jumpButton;

            _shootButton.onClick.AddListener(Shoot);
            _jumpButton.onClick.AddListener(Jump);

            _joyStick = UIManager.obj.joystick;

            foreach (SkinnedMeshRenderer smr in playerSkins)
            {
                smr.gameObject.SetActive(false);
            }
            _currentHealth = maxHealth;
            UIManager.obj.healthText.text = _currentHealth.ToString();
            UIManager.obj.healthText.color = Color.cyan;
        }

        _cam = Camera.main;
        _charCon = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked; //locks the cursor at center of screen and hides it
        UIManager.obj.tempSlider.maxValue = _maxHeat;
        photonView.RPC("SetGun", RpcTarget.All, _currentWeapon);

        //Transform newTrans = SpawnManager.obj.GetSpawnPoints();
        //transform.position = newTrans.position;
        //transform.rotation = newTrans.rotation;
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            //this is for adjusting music volume
            MusicManager.obj._audio.volume = UIManager.obj.musicSlider.value;


            //this will save the sensis and music when you open the options menu
            if (UIManager.obj.optionsScreen.activeInHierarchy)
            {
                PlayerPrefs.SetFloat("sensi", UIManager.obj.normalSensitivitySlider.value);
                PlayerPrefs.SetFloat("ads", UIManager.obj.adsSensitivitySlider.value);
                PlayerPrefs.SetFloat("music", UIManager.obj.musicSlider.value);
            }

            //this will upate the sensitivity of my player from options menu
            _sensitivity = UIManager.obj.normalSensitivitySlider.value;
            _adsSensitivity = UIManager.obj.adsSensitivitySlider.value;


            //if got hit , your vision will be damaged until autoheal
            if (_currentHealth < 30)
            {
                criticalDamage.Play();
                gotDamaged.active = true;
            }
            else
            {
                criticalDamage.Stop();
                gotDamaged.active = false;
            }

            Looking();
            Movement();


            //muzzle flash time
            if (_weapons[_currentWeapon].muzzleFlash.activeInHierarchy)
            {
               _currentMuzzleTime -= Time.deltaTime;
                if (_currentMuzzleTime <= 0)
                {
                    _weapons[_currentWeapon].muzzleFlash.SetActive(false);
                }
            }


            //if match is over ads will be false
            if (UIManager.obj.endScreenImage.activeInHierarchy)
            {
                UIManager.obj.sniperScope.SetActive(false);
                Camera.main.fieldOfView = 60f;
            }


            //shooting
            if (/*Input.GetMouseButtonDown(0) &&*/ Time.time > _canFire && !_isOverHeated)
            {
                Shoot();
                _currentHeat -= _coolRate * Time.deltaTime;
                if (UIManager.obj.sniperScope.activeInHierarchy)
                {
                    UIManager.obj.sniperScope.SetActive(false);
                    Camera.main.fieldOfView = 60f;
                }
            }
            if (/*Input.GetMouseButtonDown(0) &&*/ Time.time > _canFire && !_isOverHeated && _weapons[_currentWeapon].isAutomatic)  //shooting happens
            {
                hRecoil = Random.Range(-.05f, .05f);
                vRecoil = .1f;
                Shoot();
                _currentHeat -= _coolRate * Time.deltaTime;
            }
            if (/*Input.GetMouseButtonDown(0) ||*/ _isOverHeated)
            {
                vRecoil = 0f;
                hRecoil = 0f;
            }


            //ads
            if (/*Input.GetMouseButtonDown(1) &&*/ _currentWeapon==1)
            {
                if (!UIManager.obj.sniperScope.activeInHierarchy && !_isOverHeated)
                {
                    UIManager.obj.sniperScope.SetActive(true);
                    Camera.main.fieldOfView = 10f;
                }
                else
                {
                    Camera.main.fieldOfView = 60f;
                    UIManager.obj.sniperScope.SetActive(false);
                }
            }

          
            //when weapon is overheated ads will get false
            if (_isOverHeated)
            {
                Camera.main.fieldOfView = 60f;
                UIManager.obj.sniperScope.SetActive(false);
            }


            //gun overheating
            if (_isOverHeated)
            {
                _currentHeat -= _overHeatedCoolRate * Time.deltaTime;
            }
            else
            {
                _currentHeat -= _coolRate * Time.deltaTime;
            }
            if (_currentHeat < 0)
            {
                _currentHeat = 0;
                _isOverHeated = false;
                UIManager.obj.weaponOverheatedMessage.SetActive(false);
            }


            //will display overheat message
            UIManager.obj.tempSlider.value = _currentHeat;


            //gun switching
            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)      //this means mouse is scrolled upwards
            {
                _currentWeapon++;
                if (_currentWeapon >= _weapons.Length)
                {
                    _currentWeapon = 0;
                }
                photonView.RPC("SetGun", RpcTarget.All, _currentWeapon);
            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
            {
                _currentWeapon--;
                if (_currentWeapon < 0)
                {
                    _currentWeapon = _weapons.Length - 1;
                }
                photonView.RPC("SetGun", RpcTarget.All, _currentWeapon);
            }


            //gun switching with num keys
            for (int i = 0; i < _weapons.Length; i++)
            {
                if (Input.GetKeyDown((i + 1).ToString()))
                {
                    _currentWeapon = i;
                    photonView.RPC("SetGun", RpcTarget.All, _currentWeapon);
                }
            }


            anim.SetFloat("speed",_moveDir.magnitude);


            //strafing animations and walking animations
            if (Input.GetKey(KeyCode.S))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    anim.SetFloat("shiftSpeed", -.5f);
                }
                else
                {
                    anim.SetFloat("shiftSpeed", -1f);
                }
            }
            else
            {
                anim.SetFloat("shiftSpeed", 1f);
            }
            if (Input.GetKey(KeyCode.A))
            {
                anim.SetLayerWeight(0, 0f);
                anim.SetBool("Strafe", true);
                anim.SetBool("mirrorStrafe", true);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                anim.SetLayerWeight(0, 0f);
                anim.SetBool("Strafe", true);
                anim.SetBool("mirrorStrafe", false);
            }
            else
            {
                anim.SetLayerWeight(0, 1f);
                anim.SetBool("Strafe", false);
                anim.SetBool("mirrorStrafe", false);
            }
        }
    }

    void LateUpdate()
    {
        if (photonView.IsMine)
        {
            if (MatchManager.obj.state == MatchManager.GameState.Playing)
            {
                //this will help camera to rotate with mouse even without being the child of player
                _cam.transform.position = viewPoint.position;
                _cam.transform.rotation = viewPoint.rotation;
            }
            else
            {
                _cam.transform.position = MatchManager.obj.mapCamPoint.position;
                _cam.transform.rotation = MatchManager.obj.mapCamPoint.rotation;
            }
           
        }
    }

    #region FUNCTIONS

    void Looking()
    {
        if (!UIManager.obj.sniperScope.activeInHierarchy)
        {
            _mouseInput = new Vector2(Input.GetAxisRaw("Mouse X") + hRecoil, Input.GetAxisRaw("Mouse Y") + vRecoil) * _sensitivity;
        }
        else
        {
            _mouseInput = new Vector2(Input.GetAxisRaw("Mouse X") + hRecoil, Input.GetAxisRaw("Mouse Y") + vRecoil) * _adsSensitivity;
        }
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + _mouseInput.x, transform.rotation.eulerAngles.z));
       _verticalRotation += _mouseInput.y;

       _verticalRotation = Mathf.Clamp(_verticalRotation, -60f, 60f);           //it prevents the viewpoint to go beyond 60 degrees in up and down direction
       viewPoint.rotation = Quaternion.Euler(-_verticalRotation, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }

    void Movement()
    {
        float yVel = _movement.y;
       //_moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
       _moveDir = new Vector3(_joyStick.Horizontal, 0f, _joyStick.Vertical);

        _movement = ((transform.forward * _moveDir.z) + (transform.right * _moveDir.x)).normalized;
        _movement.y = yVel;

        if (_charCon.isGrounded)
        {
            _movement.y = 0f;
        }
        _movement.y += Physics.gravity.y * Time.deltaTime * _gravityMultiplier;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _charCon.Move(_movement * _walkSpeed * Time.deltaTime);
            anim.SetFloat("shiftSpeed", 0.5f);
        }
        else
        {
            _charCon.Move(_movement * _runSpeed * Time.deltaTime);
            anim.SetFloat("shiftSpeed", 1f);
        }
    }

    void Shoot()
    {
        Ray ray = _cam.ViewportPointToRay(new Vector3(.5f,.5f,0f));     //this means a ray is from camera's perspective to the world
        ray.origin = _cam.transform.position;                           //ray's origin is main cam's position
        if (Physics.Raycast(ray,out RaycastHit hitInfo))                //if ray hits something , it's info will be stored in hitinfo
        {
            if (hitInfo.collider.gameObject.tag == "Player")
            {
                Debug.Log("Hit : " + hitInfo.collider.gameObject.GetPhotonView().Owner.NickName);
                PhotonNetwork.Instantiate(playerHitImpact.name, hitInfo.point, Quaternion.identity);

                hitInfo.collider.gameObject.GetPhotonView().RPC("DoDamage",RpcTarget.All,photonView.Owner.NickName,_weapons[_currentWeapon].weaponDamage,PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {
                Instantiate(_hitEffect, hitInfo.point + hitInfo.normal * 0.002f, Quaternion.LookRotation(hitInfo.normal, Vector3.up));
            }
        }

        _canFire = Time.time + _weapons[_currentWeapon].fireRate;       //fire rate
        _currentHeat += _weapons[_currentWeapon].heatPerShot;           //when we fire , heat will be added
        if(_currentHeat >= _maxHeat)
        {
            _currentHeat = _maxHeat;
            _isOverHeated = true; 
            UIManager.obj.weaponOverheatedMessage.SetActive(true);
        }

        _weapons[_currentWeapon].muzzleFlash.SetActive(true);
        _currentMuzzleTime = _muzzleTime;

        _weapons[_currentWeapon].shotSound.Stop();
        _weapons[_currentWeapon].shotSound.Play();
    }

    void Jump()
    {
        //Input.GetKeyDown(KeyCode.Space)  is removed
        if (_charCon.isGrounded)
        {
            _movement.y = _jumpHeight;
        }
    }

    [PunRPC]    //RPC funtions is synchronised function
    public void DoDamage(string whoHitMe,int damage,int actor)
    {
        TakeDamage(whoHitMe,damage,actor);
    }

    public void TakeDamage(string whoHitMe,int damage,int actor)
    {
        if (photonView.IsMine)
        {
            _currentHealth -= damage;

            //blood fade effect
            UIManager.obj.GotHit();

            if (_currentHealth < maxHealth)
            {
                StartCoroutine(AutoHeal());
            }
         
            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                PlayerSpawner.obj.Die(whoHitMe);

                MatchManager.obj.UpdateStatSend(actor,0,1);
            }
            //Debug.Log(photonView.Owner.NickName+ "Got Hit by " + whoHitMe);
            if (_currentHealth <= 30)
            {
                UIManager.obj.healthText.color = Color.red;
            }
            UIManager.obj.healthText.text = _currentHealth.ToString();
        }
    }

    void SwitchWeapon()
    {
        _currentHeat = 0;
        _isOverHeated = false;
        foreach (Weapon weapon in _weapons)
        {
            weapon.gameObject.SetActive(false);
        }
        _weapons[_currentWeapon].gameObject.SetActive(true);
        _weapons[_currentWeapon].muzzleFlash.SetActive(false);

        //setting right hand away from the shoulder when not using rifle
        if (_currentWeapon == 1)
        {
            gunHolder.transform.localPosition = new Vector3(0.512f, -0.161f, 1.067f);
        }
        else if(_currentWeapon==0)
        {
            gunHolder.transform.localPosition = new Vector3(0.658f, -0.223f, 1.304f);
        }
        else if (_currentWeapon == 2)
        {
            gunHolder.transform.localPosition = new Vector3(0.658f, -0.223f, 1.304f);
        }

        //attaching left hand if rifle is equipped
        if (_currentWeapon == 1)
        {
            leftHandRig.weight++;
        }
        else
        {
            leftHandRig.weight--;
        }
    }

    IEnumerator AutoHeal()
    {
        yield return new WaitForSeconds(10);
        _currentHealth = maxHealth;
        UIManager.obj.healthText.text = _currentHealth.ToString();
        UIManager.obj.healthText.color = Color.cyan;
    }

    [PunRPC]
    public void SetGun(int gunToSwitchTo)
    {
        if (gunToSwitchTo < _weapons.Length)
        {
            _currentWeapon = gunToSwitchTo;
            SwitchWeapon();
        }
    }
    #endregion
}
