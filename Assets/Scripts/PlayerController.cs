using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    [SerializeField] private Slider slider;

    [SerializeField] private Image healthbarImage;
    [SerializeField] private GameObject ui;
    [SerializeField] private MeshRenderer mesh;
    [SerializeField] private GameObject gun;
    [SerializeField] private GameObject ionThruster;

    [SerializeField] private GameObject cameraHolder;

    [SerializeField] private Item[] items;

    private int itemIndex;
    private int previousItemIndex = -1;

    private Rigidbody rb;

    private PhotonView PV;

    private const float maxHealth = 100f;
    private float currentHealth = maxHealth;

    private PlayerManager playerManager;

    [SerializeField] private float gravity = -9.81f; //Gravity
    private CharacterController controller;

    [SerializeField] private float speed = 10; //Player speed
    private Vector3 movement; //Movement axis

    private Vector3 velocity;

    [SerializeField] private float jumpStrength;

    public LayerMask groundMask;
    private readonly float groundDistance = 0.4f;
    [SerializeField] private Transform groundCheck;
    private bool isGrounded;

    public float mouseSensitivity = 30f;

    [SerializeField] private Transform playerBody;

    [SerializeField] private GameObject sfx_Death;
    [SerializeField] private GameObject sfx_Jump;
    [SerializeField] private GameObject vfx_Explosion;
    private bool jumpSoundPlayed;

    private float movementX;
    private float movementZ;

    private float xRot;

    private void Awake()
    {
        mouseSensitivity = Settings.MouseSensitivity;

        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();

        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();

        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }
    }

    private void Start()
    {
        if (PV.IsMine)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            EquipItem(0);
            Destroy(mesh);
            Destroy(ionThruster);
            gun.layer = 6;
        }
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
            Destroy(ui);
        }
    }

    public void ChangeSensitivity()
    {
        mouseSensitivity = slider.value;
        Settings.MouseSensitivity = mouseSensitivity;
    }

    private void Update()
    {
        if (!PV.IsMine)
        {
            return;
        }

        if (transform.position.y < -10f)
        {
            Die();
        }

        for (int i = 0; i < items.Length; i++)
        {
            if (!Input.GetKeyDown((i + 1).ToString())) continue;
            EquipItem(i);
            break;
        }

        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            if (itemIndex >= items.Length - 1)
            {
                EquipItem(0);
            }
            else
            {
                EquipItem(itemIndex + 1);
            }
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            if (itemIndex <= 0)
            {
                EquipItem(items.Length - 1);
            }
            else
            {
                EquipItem(itemIndex - 1);
            }
        }

        if (velocity.y > 0)
        {
            JumpSFX();
            jumpSoundPlayed = true;
        }
        else if (velocity.y < 0)
        {
            jumpSoundPlayed = false;
        }

        MouseMovement();
        MovePlayer();

        if (Input.GetMouseButtonDown(0) && PlayerMenuManager.I.isGamePaused == false)
        {
            items[itemIndex].Use();
        }
    }

    private void MovePlayer()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded)
        {
            velocity.y = -2f; //Velocity reset because gravity sets velocity
            if (Input.GetButton("Jump") && PlayerMenuManager.I.isGamePaused == false)
            {
                velocity.y = jumpStrength;
            }
        }

        // ----- General movement -----

        if (PlayerMenuManager.I.isGamePaused == false)
        {
            movementX = Input.GetAxisRaw("Horizontal");
            movementZ = Input.GetAxisRaw("Vertical");
        }
        else
        {
            movementX = 0;
            movementZ = 0;
        }

        movement = transform.right * movementX + transform.forward * movementZ;

        controller.Move(movement * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    private void MouseMovement()
    {
        // ----- Mouse movement -----

        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivity * 3;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivity * 3;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);

        if (PlayerMenuManager.I.isGamePaused == false)
        {
            cameraHolder.transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }

    private void JumpSFX()
    {
        if (!jumpSoundPlayed)
        {
            PV.RPC("RPC_JumpSFX", RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_JumpSFX()
    {
        GameObject sfx = Instantiate(sfx_Jump, transform.position, Quaternion.identity);
        sfx.transform.parent = playerBody;
    }

    private void EquipItem(int _index)
    {
        if (_index == previousItemIndex)
            return;

        itemIndex = _index;

        items[itemIndex].itemGO.SetActive(true);

        if (previousItemIndex != -1)
        {
            items[previousItemIndex].itemGO.SetActive(false);
        }

        previousItemIndex = itemIndex;

        if (PV.IsMine)
        {
            Hashtable hash = new();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!PV.IsMine && targetPlayer == PV.Owner)
        {
            EquipItem((int)changedProps["itemIndex"]);
        }
    }

    public void TakeDamage(float damage)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    private void RPC_TakeDamage(float damage)
    {
        if (!PV.IsMine)
            return;

        currentHealth -= damage;

        healthbarImage.fillAmount = currentHealth / maxHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        PV.RPC("RPC_Die", RpcTarget.All);
        playerManager.Die();
    }

    [PunRPC]
    private void RPC_Die()
    {
        Instantiate(sfx_Death, transform.position, Quaternion.identity);
        Instantiate(vfx_Explosion, transform.position, Quaternion.identity);
    }
}
