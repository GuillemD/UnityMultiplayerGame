using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.UI;


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
    float time = 9999.0f;

    public Text newMatchName;

    public RectTransform listOfMatches;
    public GameObject matchEntryPrefab;

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

    public void ChangePlayerPrefab(PlayerController currentPlayer, int prefabIndex)
    {
        //Instantiate a new GO where the previous one was
        GameObject newPlayer = Instantiate(spawnPrefabs[prefabIndex],
            currentPlayer.gameObject.transform.position,
            currentPlayer.gameObject.transform.rotation);

        //Destroy the previous player GO
        NetworkServer.Destroy(currentPlayer.gameObject);

        //replace the connected player GO
        NetworkServer.ReplacePlayerForConnection(currentPlayer.connectionToClient, newPlayer, 0);

    }
    public string[] playerNames = new string[] { "Boy", "Girl", "Robot" };

    private void OnGUI()
    {
        if(!isNetworkActive)
        {
            playerPrefabIndex = (short)GUI.SelectionGrid(new Rect(Screen.width - 200, 10, 200, 50), playerPrefabIndex, playerNames, 3);
        }
    }




   

    private void Update()
    {
        time += Time.deltaTime;

        if (time > 5.0f)
        {
            time = 0.0f;

            NetworkManager.singleton.StartMatchMaker();
            NetworkManager.singleton.matchMaker.ListMatches(0, 10, "", true, 0, 0, OnMatchList);


        }
    }



    public void OnCreateMatchClicked()
    {
        Debug.Log("OnCreateMatchClicked" + newMatchName.text);
        NetworkManager.singleton.StartMatchMaker();
        NetworkManager.singleton.matchMaker.CreateMatch(newMatchName.text, 4, true, "", "", "", 0, 0, OnMatchCreate);
    }

    public void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        NetworkManager.singleton.StopMatchMaker();

        if (success)
        {
            NetworkManager.singleton.StartHost(matchInfo);

        }
        else
        {
            Debug.Log("OnMatchCreate failed");
        }


    }

    void OnJoinMatchClicked(UnityEngine.Networking.Types.NetworkID networkId)
    {
        NetworkManager.singleton.StartMatchMaker();
        NetworkManager.singleton.matchMaker.JoinMatch(networkId, "", "", "", 0, 0, OnMatchJoin);

    }

    public void OnMatchJoin(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        NetworkManager.singleton.StopMatchMaker();

        if (success)
        {
            NetworkManager.singleton.StartClient(matchInfo);
        }
        else
        {
            Debug.Log("OnMatchJoin failed");
        }
    }

    public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
    {

        if (success)
        {
            // Destroy previous list
            int childCount = listOfMatches.transform.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                Destroy(listOfMatches.transform.GetChild(i).gameObject);
            }

            //Insert new list of matches

            for (int i = 0; i < matches.Count; ++i)
            {
                MatchInfoSnapshot match = matches[i];
                string matchName = match.name;
                UnityEngine.Networking.Types.NetworkID networkID = match.networkId;

                GameObject gameObject = Instantiate(matchEntryPrefab, listOfMatches);
                RectTransform rect = gameObject.GetComponent<RectTransform>();

                rect.position = new Vector2(10, listOfMatches.position.y - (float)i * 50.0f);

                Text text = gameObject.GetComponentInChildren<Text>();
                text.text = "Match: " + matchName;
                Button button = gameObject.GetComponentInChildren<Button>();
                button.onClick.AddListener(delegate { OnJoinMatchClicked(networkId); });



            }



        }

        else
        {
            Debug.Log("OnMatchList failed");
        }




    }
}
