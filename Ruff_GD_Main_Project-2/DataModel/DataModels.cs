using System;
using System.Collections.Generic;
    using System.Globalization;
using Godot;
using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

namespace RuffGdMainProject.DataClasses
{
    public class UserDataModel
    {
        public UserDataModel(string uName)
        {
            UserId = Guid.NewGuid().ToString();
            UserName = uName;
        }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<CardDataModel> AllCards { get; set; }
        public List<CardDataModel> CardsDeck { get; set; }
        public List<UnitDataModel> MyUnits { get; set; }

        public void SetupUserData(List<CardDataModel> cardsList, List<UnitDataModel> unitsList)
        {
            CardsDeck = cardsList;
            MyUnits = unitsList;
        }
    }

    public class CardDataModel
    {
        public CardDataModel(long id, string name)
        {
            CardId = id;
            CardName = name;
        }
        [JsonProperty("id")]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long CardId { get; set; }
        [JsonProperty("name")]
        public string CardName { get; set; }
    }

    public class UnitDataModel
    {
        public UnitDataModel(string id, string name, int health, int tileNo = -1)
        {
            DogId = id;
            DogName = name;
            Health = health;
            Tile = tileNo;
        }
        [JsonProperty("id")]
        public string DogId { get; set; }
        [JsonProperty("name")]
        public string DogName { get; set; }
        [JsonProperty("health")]
        public int Health { get; set; }
        [JsonProperty("tileNo")]
        public int Tile { get; set; }
    }

    public class Data
    {
        [JsonProperty("createGame")]
        public GameResponse ResponseData;
    }
    public class GameResponse
    {
        [JsonProperty("auth")]
        public Auth AuthToken;
        [JsonProperty("game")]
        public MatchDataModel Game;
    }

    public class JoinGameResponse
    {
        [JsonProperty("joinGame")]
        public GameResponse ResponseData;
    }

    public class SearchGamesResponse
    {
        [JsonProperty("games")]
        public List<MatchDataModel> Matches = new List<MatchDataModel>();
    }

    public class MatchDataModel
    {
        [JsonProperty("id")]
        public Guid MatchId;
        [JsonProperty("name")]
        public string MatchName;
        [JsonProperty("player1")]
        public PlayerDataModel Player1;
        [JsonProperty("player2")]
        public PlayerDataModel Player2;
        [JsonProperty("winner")]
        public PlayerDataModel Winner;
        [JsonProperty("turn")]
        public PlayerDataModel CurrentPlayer;
        [JsonProperty("status")]
        public string Status;
        [JsonProperty("eventName")]
        public string EventName;
    }
    public class PlayerDataModel
    {
        [JsonProperty("id")]
        public string ID;
        [JsonProperty("name")]
        public string PlayerName;
        [JsonProperty("turnNo")]
        public  int Turn;

        [JsonProperty("assets")]
        public  PlayerAssets Assets;
    }
    public class PlayerAssets
    {
        [JsonProperty("dogs")]
        public List<UnitDataModel> Dogs;
        [JsonProperty("cards")]
        public List<CardDataModel> Cards;
    }
    public class Auth
    {
        [JsonProperty("token")]
        public string Token;
    }
    public class SubscriptionData : Reference
    {
        [JsonProperty("gameUpdates")]
        public GameUpdates GameUpdates;
    }
    public class GameUpdates
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("player1")]
        public PlayerDataModel Player1;

        [JsonProperty("player2")]
        public PlayerDataModel Player2;

        [JsonProperty("winner")]
        public PlayerDataModel Winner;

        [JsonProperty("turn")]
        public PlayerDataModel CurrentPlayer;

        [JsonProperty("status")]
        public string Status;

        [JsonProperty("appliedAttackCard")]
        public AppliedCardInput AppliedAttackCard;

        [JsonProperty("eventName")]
        public object EventName;
    }

    public class ClaimTurnResponse
    {
        [JsonProperty("data")]
        public ClaimTurnData Data;
    }

    public class ClaimTurnData
    {
        [JsonProperty("claimTurn")]
        public bool ClaimTurn;
    }

    public class SkipTurnResponse
    {
        [JsonProperty("data")]
        public SkipTurnData Data;
    }

    public class SkipTurnData
    {
        [JsonProperty("handOverTurn")]
        public bool SkipTurn;
    }

    public class MoveDogInput
    {
        public MoveDogInput(string id, int tile, int hp)
        {
            DogId = id;
            TileNo = tile;
            Health = hp;
        }
        [JsonProperty("id")]
        public string DogId;
        [JsonProperty("tileNo")]
        public int TileNo;
        [JsonProperty("health")]
        public int Health;
    }

    public class AppliedCardInput
    {
        public AppliedCardInput(string card, string dog, TargetDog[] targets)
        {
            DogId = dog;
            CardId = card;
            TargetDogsArray = targets;
        }

        [JsonProperty("dogId")]
        public string DogId;
        [JsonProperty("cardId")]
        public string CardId;
        [JsonProperty("targetDogs")]
        public TargetDog[] TargetDogsArray;
    }

    public class TargetDog
    {
        public TargetDog(string pId, string dId)
        {
            PlayerId = pId;
            DogId = dId;
        }
        [JsonProperty("playerId")]
        public string PlayerId;
        [JsonProperty("dogId")]
        public string DogId;
    }

    // public enum ServerEventType
    // {
    //     FORFEIT,
    //     JOINED,
    //     WINNER,
    //     TURNCHANGE,
    //     MOVE,
    //     ATTACK
    // }
}