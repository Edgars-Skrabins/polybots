using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMenuManager : MonoBehaviour
{
    public static PlayerMenuManager I;

    public bool isGamePaused;

    [SerializeField] private GameObject player;

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject crosshair;

    [SerializeField] private PhotonView PV;

    private void Awake()
    {
        if (!PV.IsMine)
        {
            return;
        }

        I = this;
    }

    private void Update()
    {
        if (!PV.IsMine)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseInput();
        }
    }

    private void PauseInput()
    {
        if (isGamePaused == false)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    public void ResumeGame()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        pauseMenu.SetActive(false);
        crosshair.SetActive(true);
        isGamePaused = false;
    }

    public void PauseGame()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        pauseMenu.SetActive(true);
        crosshair.SetActive(false);
        isGamePaused = true;
    }

    public void QuitToMainMenu()
    {
        DestroyPlayer();
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        PhotonNetwork.Disconnect();
        DestroyPlayer();
        Application.Quit();
    }

    private void DestroyPlayer()
    {
        PV.RPC("RPC_DestroyPlayer", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_DestroyPlayer()
    {
        Destroy(player);
    }

    private void OnApplicationQuit()
    {
        DestroyPlayer();
    }
}
