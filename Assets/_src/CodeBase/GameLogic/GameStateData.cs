using System;
using System.Collections.Generic;

namespace _src.CodeBase.GameLogic
{
    [Serializable]
    public class GameStateData
    {
        public string ServerIp;

        public List<ClientData> ClientDatas;

        public GameStateData()
        {
            
        }

        public GameStateData(string serverAddress, List<Player> players)
        {
            ServerIp = serverAddress;

            ClientDatas = new List<ClientData>();
            foreach (Player player in players)
            {
                ClientDatas.Add(new ClientData()
                {
                    ClientId = "Random id",
                    Score = 0,
                    Name = player.PlayerName
                });
            }
        }
    }
}