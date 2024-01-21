using Photon.Pun;
using UnityEngine;

public class SingleShotGun : Gun
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform shotPoint;
    [SerializeField] private GameObject line;

    [SerializeField] private GameObject sfx_Shot;

    private PhotonView PV;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }


    public override void Use()
    {
        Shoot();
    }

    private void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage);
            PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
        }
    }

    [PunRPC]
    private void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        GameObject sfx = Instantiate(sfx_Shot, transform.position, Quaternion.identity);
        sfx.transform.parent = transform.parent;
        var newLineObj = Instantiate(line);
        LineRenderer newLine = newLineObj.GetComponent<LineRenderer>();
        if (hitPosition != null)
        {
            newLine.SetPosition(0, shotPoint.position);
            newLine.SetPosition(1, hitPosition);
        }
        else
        {
            newLine.SetPosition(0, shotPoint.position);
            newLine.SetPosition(1, shotPoint.position * 100);
        }

        Destroy(newLineObj, 0.1f);

        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if (colliders.Length != 0)
        {
            GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.0001f,
                Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation);
            Destroy(bulletImpactObj, 5f);
            bulletImpactObj.transform.SetParent(colliders[0].transform);
        }
    }
}