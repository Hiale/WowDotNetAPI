﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web.Script.Serialization;
using WowDotNetAPI.Models;
using WowDotNetAPI.Utilities;
using System.Runtime.Serialization.Json;
using System.IO;

namespace WowDotNetAPI
{
    public enum Region
    {
        US,     //us.api.battle.net/
        EU,     //eu.api.battle.net/
        KR,     //kr.api.battle.net/
        TW,     //tw.api.battle.net/
        CN,     // ???
        SEA     //sea.api.battle.net/
    }

    public enum Locale
    {
        None,
        //US
        en_US,
        es_MX,
        pt_BR,
        //EU
        en_GB,
        de_DE,
        es_ES,
        fr_FR,
        it_IT,
        pl_PL,
        pt_PT,
        ru_RU,
        //KR
        ko_KR,
        //TW
        zh_TW,
        //CN
        zh_CN
    }

    [Flags]
    public enum CharacterOptions
    {
        None = 0,
        GetGuild = 1,
        GetStats = 2,
        GetTalents = 4,
        GetItems = 8,
        GetReputation = 16,
        GetTitles = 32,
        GetProfessions = 64,
        GetAppearance = 128,
        GetPetSlots = 256,
        GetMounts = 512,
        GetPets = 1024,
        GetAchievements = 2048,
        GetProgression = 4096,
        GetFeed = 8192,
        GetPvP = 16384,
        GetQuests = 32768,
        GetHunterPets = 65536,
        GetEverything = GetGuild | GetStats | GetTalents | GetItems | GetReputation | GetTitles
        | GetProfessions | GetAppearance | GetPetSlots | GetMounts | GetPets
        | GetAchievements | GetProgression | GetFeed | GetPvP | GetQuests | GetHunterPets
    }

    [Flags]
    public enum GuildOptions
    {
        None = 0,
        GetMembers = 1,
        GetAchievements = 2,
        GetNews = 4,
        GetEverything = GetMembers | GetAchievements | GetNews
    }

    public class WowExplorer : IExplorer
    {
        public Region Region { get; set; }
        public Locale Locale { get; set; }
        public string APIKey { get; set; }

        public string Host { get; set; }

        public WowExplorer(Region region, Locale locale, string apiKey)
        {
            Region = region;
            Locale = locale;
            APIKey = apiKey;

            switch (Region)
            {
                case Region.EU:
                    Host = "https://eu.api.battle.net";
                    break;
                case Region.KR:
                    Host = "https://kr.api.battle.net";
                    break;
                case Region.TW:
                    Host = "https://tw.api.battle.net";
                    break;
                case Region.CN:
                    Host = "https://www.battlenet.com.cn";
                    break;
                case Region.US:
                default:
                    Host = "https://us.api.battle.net";
                    break;
            }
        }

        #region Character

        public Character GetCharacter(string realm, string name)
        {
            return GetCharacter(Region, realm, name, CharacterOptions.None);
        }

        public Character GetCharacter(Region region, string realm, string name)
        {
            return GetCharacter(region, realm, name, CharacterOptions.None);
        }

        public Character GetCharacter(string realm, string name, CharacterOptions characterOptions)
        {
            return GetCharacter(Region, realm, name, characterOptions);
        }

        public Character GetCharacter(Region region, string realm, string name, CharacterOptions characterOptions)
        {
            Character character;

            TryGetData<Character>(
                string.Format(@"{0}/wow/character/{1}/{2}?locale={3}{4}&apikey={5}", Host, realm, name, Locale, CharacterUtility.buildOptionalQuery(characterOptions), APIKey),
                out character);

            return character;
        }

        #endregion

        #region Guild

        public Guild GetGuild(string realm, string name)
        {
            return GetGuild(Region, realm, name, GuildOptions.None);
        }

        public Guild GetGuild(Region region, string realm, string name)
        {
            return GetGuild(region, realm, name, GuildOptions.None);
        }

        public Guild GetGuild(string realm, string name, GuildOptions realmOptions)
        {
            return GetGuild(Region, realm, name, realmOptions);
        }

        public Guild GetGuild(Region region, string realm, string name, GuildOptions realmOptions)
        {
            Guild guild;

            TryGetData<Guild>(
                string.Format(@"{0}/wow/guild/{1}/{2}?locale={3}{4}&apikey={5}", Host, realm, name, Locale, GuildUtility.buildOptionalQuery(realmOptions), APIKey),
                out guild);

            return guild;
        }

        #endregion

        #region Realms
        public IEnumerable<Realm> GetRealms()
        {
            RealmsData realmsData;

            TryGetData<RealmsData>(
                string.Format(@"{0}/wow/realm/status?locale={1}&apikey={2}", Host, Locale, APIKey),
                out realmsData);

            return (realmsData != null) ? realmsData.Realms : null;
        }

        #endregion

        #region Auctions

        public Auctions GetAuctions(string realm)
        {
            AuctionFiles auctionFiles;

            TryGetData<AuctionFiles>(
                string.Format(@"{0}/wow/auction/data/{1}?locale={2}&apikey={3}", Host, realm.ToLower().Replace(' ', '-'), Locale, APIKey),
                out auctionFiles);

            if (auctionFiles != null)
            {
                string url = "";
                foreach (AuctionFile auctionFile in auctionFiles.Files)
                {
                    url = auctionFile.URL;
                }

                Auctions auctions;

                TryGetData<Auctions>(url, out auctions);

                return auctions;
            }

            return null;
        }

        #endregion

        #region Items
        public Item GetItem(int id)
        {
            Item item;

            TryGetData<Item>(
                string.Format(@"{0}/wow/item/{1}?locale={2}&apikey={3}", Host, id, Locale, APIKey),
                out item);

            return item;
        }

        //extended by Hiale
        public Item GetItemWithoutKey(int id)
        {
            Item item;

            TryGetData<Item>(
                string.Format(@"http://{0}.battle.net/api/wow/item/{1}?locale={2}", Region.ToString().ToLower(), id, Locale),
                out item);

            return item;
        }
        //extended by Hiale

        public IEnumerable<ItemClassInfo> GetItemClasses()
        {
            ItemClassData itemclassdata;

            TryGetData<ItemClassData>(
                string.Format(@"{0}/wow/data/item/classes?locale={1}&apikey={2}", Host, Locale, APIKey),
                out itemclassdata);

            return (itemclassdata != null) ? itemclassdata.Classes : null;
        }

        #endregion

        #region CharacterRaceInfo
        public IEnumerable<CharacterRaceInfo> GetCharacterRaces()
        {
            CharacterRacesData charRacesData;

            TryGetData<CharacterRacesData>(
                string.Format(@"{0}/wow/data/character/races?locale={1}&apikey={2}", Host, Locale, APIKey),
                out charRacesData);

            return (charRacesData != null) ? charRacesData.Races : null;
        }
        #endregion

        #region CharacterClassInfo
        public IEnumerable<CharacterClassInfo> GetCharacterClasses()
        {
            CharacterClassesData characterClasses;

            TryGetData<CharacterClassesData>(
                string.Format(@"{0}/wow/data/character/classes?locale={1}&apikey={2}", Host, Locale, APIKey),
                out characterClasses);

            return (characterClasses != null) ? characterClasses.Classes : null;
        }
        #endregion

        #region GuildRewardInfo
        public IEnumerable<GuildRewardInfo> GetGuildRewards()
        {
            GuildRewardsData guildRewardsData;
            
            TryGetData<GuildRewardsData>(
                string.Format(@"{0}/wow/data/guild/rewards?locale={1}&apikey={2}", Host, Locale, APIKey),
                out guildRewardsData);
            
            return (guildRewardsData != null) ? guildRewardsData.Rewards : null;
        }
        #endregion

        #region GuildPerkInfo
        public IEnumerable<GuildPerkInfo> GetGuildPerks()
        {
            GuildPerksData guildPerksData;

            TryGetData<GuildPerksData>(
                 string.Format(@"{0}/wow/data/guild/perks?locale={1}&apikey={2}", Host, Locale, APIKey), 
                 out guildPerksData);

            return (guildPerksData != null) ? guildPerksData.Perks : null;
        }
        #endregion

        #region Achievements
        public AchievementInfo GetAchievement(int id)
        {
            AchievementInfo achievement;

            TryGetData<AchievementInfo>(
                string.Format(@"{0}/wow/achievement/{1}?locale={2}&apikey={3}", Host, id, Locale, APIKey),
                out achievement);

            return achievement;
        }

        public IEnumerable<AchievementList> GetAchievements()
        {
            AchievementData achievementData;
            
            TryGetData<AchievementData>(
                string.Format(@"{0}/wow/data/character/achievements?locale={1}&apikey={2}", Host, Locale, APIKey),
                out achievementData);

            return (achievementData != null) ? achievementData.Lists : null;
        }

        public IEnumerable<AchievementList> GetGuildAchievements()
        {
            AchievementData achievementData;

            TryGetData<AchievementData>(
                string.Format(@"{0}/wow/data/guild/achievements?locale={1}&apikey={2}", Host, Locale, APIKey),
                out achievementData);

            return (achievementData != null) ? achievementData.Lists : null;
        }

        #endregion

        #region Battlegroups
        public IEnumerable<BattlegroupInfo> GetBattlegroupsData()
        {
            BattlegroupData battlegroupData;            
            
            TryGetData<BattlegroupData>(
                string.Format(@"{0}/wow/data/battlegroups/?locale={1}&apikey={2}", Host, Locale, APIKey), 
                out battlegroupData);

            return (battlegroupData != null) ? battlegroupData.Battlegroups : null;
        }
        #endregion

        #region Challenges
        public Challenges GetChallenges(string realm)
        {
            Challenges challenges;

            TryGetData<Challenges>(
                string.Format(@"{0}/wow/challenge/{1}?locale={2}&apikey={3}", Host, realm, Locale, APIKey), 
                out challenges);

            return challenges;
        }
        #endregion

        private T GetData<T>(string url) where T : class
        {
            return JsonUtility.FromJSON<T>(url);
        }

        private void TryGetData<T>(string url, out T requestedObject) where T : class
        {
            try
            {
                requestedObject = JsonUtility.FromJSON<T>(url);
            }
            catch (Exception ex)
            {                
                requestedObject = null;
                throw ex;
            }
        }
    }
}
