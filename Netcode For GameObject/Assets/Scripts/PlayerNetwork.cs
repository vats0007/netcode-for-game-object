using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] Transform spawnedObject;
    private Transform spawnedObjectTransform;

    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(
        new MyCustomData
    {
        _int = 56,
        _bool = true,

    }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    public struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        public FixedString128Bytes msg;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref msg);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) => {

            Debug.Log(OwnerClientId + "; " + newValue._int + "; " + newValue._bool + "; " + newValue.msg);
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;



        if (Input.GetKeyDown(KeyCode.T))
        {

            spawnedObjectTransform =  Instantiate(spawnedObject);
            spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);
            //TestServerRpc(new ServerRpcParams());
            //TestClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { 0 } } });
            /*
            randomNumber.Value = new MyCustomData
            {
                _int = Random.Range(0, 100),
                _bool = false,
                msg = "Hello Foks",
            };
            */
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            spawnedObjectTransform.GetComponent<NetworkObject>().Despawn(true);
            Destroy(spawnedObject.gameObject);
        }

        Vector3 moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDir.z += 1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z -= 1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x -= 1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x += 1f;

        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }


    //Also both RPC must be inside an NetworkBehaviour
    //need to Write below line to use ServerRpc
    [ServerRpc]
    //To use ServerRPC function name must end with "ServerRpc" suffix.
    //ServerRpc only runs on Server even if it is called from client.
    //Host works as both client and server
    private void TestServerRpc(ServerRpcParams serverRpcParams)
    {
        Debug.Log("TestServerRpc" + OwnerClientId + "; " + serverRpcParams.Receive.SenderClientId);
    }

    //Unity also have Client RPC if a function is called from server that is ClientRPC then the functions will run on every object of version client.
    //Can send to one perticular client using their ClientId.
    [ClientRpc]
    private void TestClientRpc(ClientRpcParams clientRpcParams)
    {
        Debug.Log("TestClientRpc");
    }



    //Host is both client and server so it host calls ClientRpc then function will run on Client as well as on Host.
}
