using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MsgTypes
{
    public const short PlayerPrefabSelect = MsgType.Highest + 1;
    public class PlayerPrefabMsg : MessageBase
    {
        public short controllerId;
        public short prefabIndex;
    }
}
public class CustomNetworkManager : NetworkManager
{
    public short playerPrefabIndex;
    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler(MsgTypes.PlayerPrefabSelect, OnPrefabResponse);
        base.OnStartServer();

    }
    public override void OnClientConnect(NetworkConnection conn)
    {
        client.RegisterHandler(MsgTypes.PlayerPrefabSelect, OnPrefabRequest);
        base.OnClientConnect(conn);
    }
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        MsgTypes.PlayerPrefabMsg msg = new MsgTypes.PlayerPrefabMsg();
        msg.controllerId = playerControllerId;
        NetworkServer.SendToClient(conn.connectionId, MsgTypes.PlayerPrefabSelect, msg);
    }

    private void OnPrefabRequest(NetworkMessage netMsg)
    {
        MsgTypes.PlayerPrefabMsg msg = netMsg.ReadMessage<MsgTypes.PlayerPrefabMsg>();
        msg.prefabIndex = playerPrefabIndex;
        client.Send(MsgTypes.PlayerPrefabSelect, msg);
    }

    private void OnPrefabResponse(NetworkMessage netMsg)
    {
        MsgTypes.PlayerPrefabMsg msg = netMsg.ReadMessage<MsgTypes.PlayerPrefabMsg>();
        playerPrefab = spawnPrefabs[msg.prefabIndex];
        base.OnServerAddPlayer(netMsg.conn, msg.controllerId);
    }

    public string[] playerNames = new string[] { "Boy", "Girl", "Robot" };

    private void OnGUI()
    {
        if(!isNetworkActive)
        {
            playerPrefabIndex = (short)GUI.SelectionGrid(new Rect(Screen.width - 200, 10, 200, 50), playerPrefabIndex, playerNames, 3);
        }
    }

}
