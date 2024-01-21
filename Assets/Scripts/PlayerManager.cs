using System.IO;
using Photon.Pun;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private GameObject controller;
    private PhotonView PV;

    private void Awake()
    {
        InitializeReferences();
    }

    private void Start()
    {
        InitializeMultiplayerSetup();
    }

    private void InitializeReferences()
    {
        PV = GetComponent<PhotonView>();
    }

    private void InitializeMultiplayerSetup()
    {
        if (PV.IsMine)
        {
            CreateController();
        }
    }

    private void CreateController()
    {
        Transform spawnPoint = SpawnManager.I.GetSpawnpoint();
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnPoint.position,
            spawnPoint.rotation, 0, new object[] { PV.ViewID });
    }

    public void Die()
    {
        PhotonNetwork.Destroy(controller);
        CreateController();
    }
}
