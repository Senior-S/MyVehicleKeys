using MyVehicleKeys.Model;
using Newtonsoft.Json;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;

namespace MyVehicleKeys
{
    public class Utils
    {
        public static List<PlayerKeys> GetKeys()
        {
            string jsonText = File.ReadAllText(path);
            List<PlayerKeys> sad = JsonConvert.DeserializeObject<List<PlayerKeys>>(jsonText);

            return sad;
        }

        public static List<uint> GetPlayerKeys(string playerId)
        {
            List<PlayerKeys> pks = GetKeys();
            for (int i = 0; i < pks.Count; i++)
            {
                if (pks[i].Id == playerId)
                {
                    return pks[i].Vehicles;
                }
            }
            return null;
        }

        public static int GetPlayerMaxKeys(string playerId)
        {
            List<PlayerKeys> pks = GetKeys();
            for (int i = 0; i < pks.Count; i++)
            {
                if (pks[i].Id == playerId)
                {
                    return pks[i].MaxVehicleKeys;
                }
            }
            return 0;
        }

        public static void SetPlayerMaxKeys(string playerId, int newMaxKeys)
        {
            List<PlayerKeys> pks = GetKeys();
            for (int i = 0; i < pks.Count; i++)
            {
                if (pks[i].Id == playerId)
                {
                    pks[i].MaxVehicleKeys = newMaxKeys;
                    string fileJson = JsonConvert.SerializeObject(pks.ToArray(), Formatting.Indented);

                    File.WriteAllText(path, fileJson);
                }
            }
        }

        public static void CreateInitialFile()
        {
            PlayerKeys ax = new PlayerKeys
            {
                Id = "DEFAULT",
                Vehicles = new List<uint>(),
                MaxVehicleKeys = 3
            };
            List<PlayerKeys> zz = new List<PlayerKeys> { ax };
            string asd = JsonConvert.SerializeObject(zz.ToArray(), Formatting.Indented);
            File.AppendAllText(path, asd);
        }

        public static void AddPlayer(PlayerKeys playerKey)
        {
            string jsonText = File.ReadAllText(path);
            var sad = JsonConvert.DeserializeObject<List<PlayerKeys>>(jsonText);

            sad.Add(playerKey);

            string fileJson = JsonConvert.SerializeObject(sad.ToArray(), Formatting.Indented);

            File.WriteAllText(path, fileJson);
        }

        public static string AddPlayerKey(uint key, string playerId)
        {
            string s = "error";
            List<PlayerKeys> pks = GetKeys();
            for (int i = 0; i < pks.Count; i++)
            {
                if (pks[i].Id == playerId)
                {
                    if (pks[i].Vehicles.Count < pks[i].MaxVehicleKeys)
                    {
                        pks[i].Vehicles.Add(key);
                        string fileJson = JsonConvert.SerializeObject(pks.ToArray(), Formatting.Indented);

                        File.WriteAllText(path, fileJson);
                        return null;
                    }
                    else
                    {
                        return s;
                    }
                }
            }

            return s;
        }

        public static string RemovePlayerKey(uint key, string playerId)
        {
            string s = "error";
            List<PlayerKeys> pks = GetKeys();
            for (int i = 0; i < pks.Count; i++)
            {
                if (pks[i].Id == playerId)
                {
                    if (pks[i].Vehicles.Contains(key))
                    {
                        pks[i].Vehicles.Remove(key);
                        string fileJson = JsonConvert.SerializeObject(pks.ToArray(), Formatting.Indented);

                        File.WriteAllText(path, fileJson);
                        return null;
                    }
                    else
                    {
                        string nokey = "nokey";
                        return nokey;
                    }
                }
            }
            return s;
        }

        public static string CheckVehicleOwner(uint key)
        {
            List<PlayerKeys> pks = GetKeys();
            for (int i = 0; i < pks.Count; i++)
            {
                if (pks[i].Vehicles.Contains(key))
                {
                    return pks[i].Id;
                }
            }
            return null;
        }

        public static readonly string path = $"{Environment.CurrentDirectory}/Servers/{Dedicator.serverID}/OpenMod/plugins/SS.MyVehicleKeys/PlayerKeys.json";
    }
}