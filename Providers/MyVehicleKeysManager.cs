using MyVehicleKeys.Model;
using OpenMod.API.Ioc;
using OpenMod.API.Plugins;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyVehicleKeys.Providers
{
    [ServiceImplementation(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton, Priority = OpenMod.API.Prioritization.Priority.Low)]
    public class MyVehicleKeysManager
    {
        private Keys m_KeysCache;

        private readonly IPluginAccessor<MyVehicleKeys> m_PluginAccesor;

        public MyVehicleKeysManager(IPluginAccessor<MyVehicleKeys> pluginAccessor)
        {
            m_PluginAccesor = pluginAccessor;
        }

        public async Task<List<PlayerKeys>> GetKeysAsync()
        {
            if (m_KeysCache == null)
            {
                await ReadData();
            }

            return m_KeysCache.PKeys;
        }

        public async Task<List<uint>> GetPlayerKeysAsync(string playerId)
        {
            List<PlayerKeys> pks = await GetKeysAsync();
            for (int i = 0; i < pks.Count; i++)
            {
                if (pks[i].Id == playerId)
                {
                    return pks[i].Vehicles;
                }
            }
            return null;
        }

        public async Task<int> GetPlayerMaxKeysAsync(string playerId)
        {
            List<PlayerKeys> pks = await GetKeysAsync();
            for (int i = 0; i < pks.Count; i++)
            {
                if (pks[i].Id == playerId)
                {
                    return pks[i].MaxVehicleKeys;
                }
            }
            return 0;
        }

        public async Task SetPlayerMaxKeysAsync(string playerId, int newMaxKeys)
        {
            List<PlayerKeys> pks = await GetKeysAsync();
            for (int i = 0; i < pks.Count; i++)
            {
                if (pks[i].Id == playerId)
                {
                    m_KeysCache.PKeys.Remove(pks[i]);
                    pks[i].MaxVehicleKeys = newMaxKeys;
                    m_KeysCache.PKeys.Add(pks[i]);

                    await m_PluginAccesor.Instance.DataStore.SaveAsync(MVKKEY, m_KeysCache);
                }
            }
        }

        public async Task AddPlayer(PlayerKeys playerKey)
        {
            List<PlayerKeys> pks = await GetKeysAsync();
            pks.Add(playerKey);

            await m_PluginAccesor.Instance.DataStore.SaveAsync(MVKKEY, m_KeysCache);
        }

        public async Task<string> AddPlayerKey(uint key, string playerId)
        {
            string s = "error";
            List<PlayerKeys> pks = await GetKeysAsync();
            for (int i = 0; i < pks.Count; i++)
            {
                if (pks[i].Id == playerId)
                {
                    if (pks[i].Vehicles.Count < pks[i].MaxVehicleKeys)
                    {
                        m_KeysCache.PKeys.Remove(pks[i]);
                        pks[i].Vehicles.Add(key);
                        m_KeysCache.PKeys.Add(pks[i]);

                        await m_PluginAccesor.Instance.DataStore.SaveAsync(MVKKEY, m_KeysCache);
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

        public async Task<string> RemovePlayerKey(uint key, string playerId)
        {
            string s = "error";
            List<PlayerKeys> pks = await GetKeysAsync();
            for (int i = 0; i < pks.Count; i++)
            {
                if (pks[i].Id == playerId)
                {
                    if (pks[i].Vehicles.Contains(key))
                    {
                        m_KeysCache.PKeys.Remove(pks[i]);
                        pks[i].Vehicles.Remove(key);
                        m_KeysCache.PKeys.Add(pks[i]);

                        await m_PluginAccesor.Instance.DataStore.SaveAsync(MVKKEY, m_KeysCache);
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

        public async Task<string> CheckVehicleOwner(uint key)
        {
            List<PlayerKeys> pks = await GetKeysAsync();
            for (int i = 0; i < pks.Count; i++)
            {
                if (pks[i].Vehicles.Contains(key))
                {
                    return pks[i].Id;
                }
            }
            return null;
        }

        private async Task ReadData()
        {
            m_KeysCache = await m_PluginAccesor.Instance.DataStore.LoadAsync<Keys>(MVKKEY)
                            ?? new Keys();
        }


        internal const string MVKKEY = "keys";
    }
}
