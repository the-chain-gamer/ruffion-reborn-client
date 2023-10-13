using Godot;
using System;
// To use NewtonsoftJsonSerializer, add a reference to NuGet package GraphQL.Client.Serializer.Newtonsoft
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using System.Collections.Generic;
using System.Threading.Tasks;
using RuffGdMainProject.GameScene;
using RuffGdMainProject.GridSystem;
using RuffGdMainProject.UiSystem;

namespace RuffGdMainProject.DataClasses
{
    public class MultiplayerManager : Node2D
    {
        private SubscriptionController subscriptionController;
        private List<MatchDataModel> availableRooms;
        private MatchDataModel JoinedRoom;
        private const string URL = "http://localhost:3000/graphql";
        private string AuthToken = "";
        private bool isGameStarted = false;
        private GraphQLHttpClientOptions graphQLHttpClientOptions = new GraphQLHttpClientOptions {
            EndPoint = new Uri(URL)
        };

        public string PlayerId { get; private set; }

        public GraphQLHttpClientOptions GraphQLHttpClientOptions { get => graphQLHttpClientOptions; set => graphQLHttpClientOptions = value; }

        public override void _Ready()
        {
            subscriptionController = GetNode<SubscriptionController>("SubscriptionManager");
            subscriptionController.ClientConnected += OnClientConnected;
        }

        private void OnClientConnected(string clientId)
        {
            GD.Print("New client connected:", clientId);
            // Handle the new client connection event
        }

        //Searching for rooms
        public async Task SearchForRooms(UserDataModel playerData)
        {
            GD.Print("In SearchForRooms function");

            var graphQLClient = new GraphQLHttpClient(GraphQLHttpClientOptions, new NewtonsoftJsonSerializer());

            var graphQLRequest = new GraphQLRequest
            {
                Query = @"query {
                    games {id, name, status}
                }"
            };

            var graphQLResponse = await graphQLClient.SendQueryAsync<SearchGamesResponse>(graphQLRequest);

            if (graphQLResponse.Errors != null)
            {
                GD.Print($"GraphQL request failed: {graphQLResponse.Errors[0].Message}");
            }
            else
            {
                availableRooms = graphQLResponse.Data.Matches;
                GD.Print("Total Games =  " + graphQLResponse.Data.Matches.Count);
            }

            GD.Print("allRooms Count = " + availableRooms.Count);

            if (availableRooms.Count > 0)
            {
                var rooms = availableRooms.FindAll(x => x.Status == "WAITING");
                GD.Print("available are Rooms Count  = " + rooms.Count);

                if (rooms.Count > 0)
                {
                    GD.Print("Rooms are available");
                    var random = new RandomNumberGenerator();
                    random.Randomize();
                    var roomNum = random.RandiRange(1, rooms.Count);
                    var room = rooms[roomNum - 1];
                    var joinedRoom = JoinRoom(playerData, room);
                }
                else
                {
                    GD.Print("No rooms found");
                    var createdRoom = CreateRoom(playerData);
                }
            }
            else
            {
                GD.Print("No rooms found so creating one");
                var createdRoom = CreateRoom(playerData);
            }
        }
        public async Task JoinRoom(UserDataModel playerData, MatchDataModel room)
        {
            GD.Print("roomID  = " + room.MatchId);

            var graphQLClient = new GraphQLHttpClient(GraphQLHttpClientOptions, new NewtonsoftJsonSerializer());

            var graphQLRequest = new GraphQLRequest
            {
                Query = @"
                    mutation ($player: PlayerInput!, $gameId: ID!, $assets: AssetsInput!) {
                        joinGame(player: $player, gameId: $gameId, assets: $assets) {
                            auth {
                                token
                            }
                            game {
                                ...Game
                            }
                        }
                    }
                    fragment Player on Player {
                        id
                        name
                        turnNo
                        assets{...Assets}
                    }
                    fragment Assets on Assets{
                        dogs {
                            id
                            name
                            health
                            tileNo
                        }
                        cards {
                            id
                            name
                        }
                    }
                    fragment Game on Game {
                        id
                        name
                        player1 {
                            ...Player
                        }
                        player2 {
                            ...Player
                        }
                        winner {
                            ...Player
                        }
                        turn {
                            ...Player
                        }
                        status
                        eventName
                    }",
                Variables = new {
                    player = new { id = playerData.UserId, name = playerData.UserName },
                    gameId = room.MatchId,
                    assets = new { dogs = playerData.MyUnits, cards = playerData.CardsDeck }
                }
            };

            GD.Print("Trying to send Join Game Query");

            var mutationResponse = await graphQLClient.SendMutationAsync<JoinGameResponse>(graphQLRequest);

            if (mutationResponse.Errors != null)
            {
                GD.Print($"Join Game GraphQL request failed: {mutationResponse.Errors[0].Message}");
            }
            else
            {
                GD.Print("Joined Game ID =  " + mutationResponse.Data.ResponseData.Game.MatchId);
                AuthToken = mutationResponse.Data.ResponseData.AuthToken.Token;
                GD.Print("Joined Game Auth Token =  " + AuthToken);
                JoinedRoom = mutationResponse.Data.ResponseData.Game;
                PlayerId = playerData.UserId;
                GD.Print("mutationResponse.Data = " + mutationResponse.Data.ResponseData.Game.Status);
                Logger.UiLogger.Log(Logger.LogLevel.INFO, "Joined room & looking for subs");
                if(mutationResponse.Data.ResponseData.Game.Status.Equals("STARTED") || mutationResponse.Data.ResponseData.Game.EventName.Equals("JOINED"))
                {
                    StartupScript.Startup.LoadOppnantImgs(mutationResponse.Data.ResponseData.Game.Player1, mutationResponse.Data.ResponseData.Game.Player2);
                    Logger.UiLogger.Log(Logger.LogLevel.INFO, "I have Joined this room");
                    //Mark Player 1 as Local Player
                    //load initial data for both players.
                    //Start Game
                    //Start Turn Timer
                    isGameStarted = true;
                    StartupScript.Startup.HideWithDelay(mutationResponse.Data.ResponseData);
                }
                SubscribeForEvents();
            }
        }

        public async Task CreateRoom(UserDataModel playerData)
        {
            var data = new MatchDataModel();
            GD.Print("CreateRoom Func");

            var graphQLClient = new GraphQLHttpClient(GraphQLHttpClientOptions, new NewtonsoftJsonSerializer());

            var graphQLRequest = new GraphQLRequest
            {
                Query = @"mutation($player: PlayerInput!, $gameName: String!, $assets: AssetsInput!) {
                    createGame(player: $player, gameName: $gameName, assets: $assets) {
                        auth {
                            token
                        }
                        game {
                            ...Game
                        }
                    }
                }
                fragment Player on Player {
                    id
                    name
                    turnNo
                    assets{...Assets}
                }
                fragment Assets on Assets{
                    dogs {
                        id
                        name
                        health
                        tileNo
                    }
                    cards {
                        id
                        name
                    }
                }
                fragment Game on Game {
                    id
                    name
                    player1 {
                        ...Player
                    }
                    player2 {
                        ...Player
                    }
                    winner {
                        ...Player
                    }
                    turn {
                        ...Player
                    }
                    status
                    eventName
                }",
                Variables = new { player = new { id = playerData.UserId, name = playerData.UserName },
                    gameName = "New game", assets = new { dogs = playerData.MyUnits, cards = playerData.CardsDeck }
                }
            };

            GD.Print("Trying to send Create Room Query");

            var mutationResponse = await graphQLClient.SendMutationAsync<Data>(graphQLRequest);

            GD.Print("Checking create room response");

            if (mutationResponse.Errors != null)
            {
                GD.Print($"GraphQL request failed: {mutationResponse.Errors[0].Message}");
            }
            else
            {
                GD.Print("SUCCESSSSS");
                GD.Print("mutationResponse.Data.token = " + mutationResponse.Data);

                if (mutationResponse.Data != null)
                {
                    GD.Print("DATA HEGA");
                    GD.Print("Game ID =  " + mutationResponse.Data);
                    AuthToken = mutationResponse.Data.ResponseData.AuthToken.Token;
                    GD.Print("Joined Game Auth Token =  " + AuthToken);
                    JoinedRoom = mutationResponse.Data.ResponseData.Game;
                    PlayerId = playerData.UserId;
                    GD.Print("CALLING Subscription");
                    SubscribeForEvents();
                }
                else
                {
                    GD.Print("NO SUCCESSssss");
                }
            }
        }

        public async void SubscribeForEvents()
        {
            var abc = subscriptionController.ConnectWebSocket(AuthToken);
        }

        public void RecieveSubscriptionData(SubscriptionData data)
        {
            // GD.Print("RecieveSubscriptionData =  " + data.GameUpdates.Player1.Assets.Dogs.Count);
            Logger.UiLogger.Log(Logger.LogLevel.INFO, "In Sub response main");
            Logger.UiLogger.Log(Logger.LogLevel.INFO, "data.GameUpdates.Status = " + data.GameUpdates.Status);
            GD.Print("Server Sent Event Type: " + data.GameUpdates.EventName.ToString());
            string eventName = data.GameUpdates.EventName.ToString();
            if(!isGameStarted && (data.GameUpdates.Status.Equals("STARTED") || eventName.Equals("JOINED")))
            {
                StartupScript.Startup.LoadOppnantImgs(data.GameUpdates.Player1, data.GameUpdates.Player2);
                isGameStarted = true;
                Logger.UiLogger.Log(Logger.LogLevel.INFO, "I have Joined this room");
                //Mark Player 1 as Local Player
                //load initial data for both players.
                //Start Game
                //Start Turn Timer
                StartupScript.Startup.HideWithDelay(data);
            }
            else if(eventName.Equals("TURNCHANGE"))
            {
                GridManager.GM.UiController.StopTimer();
                GD.Print("It's a turn change event now we have to check who's turn it is");
                // GD.Print("It's a turn change event now we have to check who's turn it is?");
                string currentPlayerID = data.GameUpdates.CurrentPlayer.ID;
                var player = GridManager.GM.TM.GetPlayerByID(currentPlayerID);
                var totalTurns = data.GameUpdates.Player1.Turn + data.GameUpdates.Player2.Turn;
                GD.Print("data.GameUpdates.Player1.Turn from Server = " + data.GameUpdates.Player1.Turn);
                GD.Print("data.GameUpdates.Player2.Turn from Server = " + data.GameUpdates.Player2.Turn);
                GD.Print("Total Turns from Server = " + totalTurns);
                GridManager.GM.TM.ChangeTurn(player, data.GameUpdates.CurrentPlayer.Turn, totalTurns);
            }
            else if(eventName.Equals("MOVE"))
            {
                GD.Print("Unit moved!!!");
                GridManager.GM.MoveUnits(data.GameUpdates.CurrentPlayer);
            }
            else if(eventName.Equals("WINNER"))
            {
                Logger.UiLogger.Log(Logger.LogLevel.INFO, "The winnder is = " + data.GameUpdates.Winner.ID);
                GD.Print("The winnder is = " + data.GameUpdates.Winner.ID);
            }
            else if(eventName.Equals("FORFEIT"))
            {
                Logger.UiLogger.Log(Logger.LogLevel.INFO, "Other Player Disconnected");
                GD.Print("Other Player Disconnected" );
                StartupScript.Startup.ReturnToMenu();
            }
            else if(eventName.Equals("ATTACK"))
            {
                Logger.UiLogger.Log(Logger.LogLevel.INFO, "Unit Attacked!!!");
                GD.Print("trying to convert CardInputArray from OBJECT" );
                // AppliedCardInput[] objs = data.GameUpdates.AppliedAttackCards as AppliedCardInput[];
                GD.Print("data.GameUpdates.AppliedAttackCards = " + data.GameUpdates.AppliedAttackCard);
                // GD.Print("data.GameUpdates.AppliedAttackCards.TargetDogs[0].ID = " + data.GameUpdates.AppliedAttackCard.TargetDogsArray[0].DogId);
                var card = StartupScript.Startup.DB.GetDataByID(Convert.ToInt32(data.GameUpdates.AppliedAttackCard.CardId));
                if(card.CardName.Equals("Bubbles' Pick 'n' Drop") || card.CardName.Equals("Miss Cake By The Ocean"))
                {
                    List<PlayerDataModel> players = new List<PlayerDataModel>();
                    players.Add(data.GameUpdates.Player1);
                    players.Add(data.GameUpdates.Player2);
                    GridManager.GM.CM.MultiplayerAttack(data.GameUpdates.AppliedAttackCard, data.GameUpdates.CurrentPlayer, players);
                }
                else
                {
                    GridManager.GM.CM.MultiplayerAttack(data.GameUpdates.AppliedAttackCard, data.GameUpdates.CurrentPlayer);
                }
            }
        }

        public async Task ClaimTurnRequest()
        {
            GD.Print("ClaimTurnFunc Func");
            var graphQlEndpoint = new Uri(URL);
            var graphQLClient = new GraphQLHttpClient(graphQlEndpoint, new NewtonsoftJsonSerializer());
            graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + AuthToken);
            var graphQLRequest = new GraphQLRequest
            {
                Query = @"mutation {
                    claimTurn
                }"
            };

            GD.Print("Trying to send ClaimTurn Mutation");

            var mutationResponse = await graphQLClient.SendMutationAsync<ClaimTurnResponse>(graphQLRequest);

            GD.Print("Checking ClaimTurn response");

            if (mutationResponse.Errors != null)
            {
                GD.Print($"GraphQL request failed: {mutationResponse.Errors[0].Message}");
            }
            else
            {
                GD.Print("TURN Claimed )))");
                GD.Print("CALIM TURN  )))" + mutationResponse);
                if (mutationResponse.Data.Data.ClaimTurn)
                {
                    GD.Print("Your Turn Now");
                    // GridManager.GM.TM.ChangeTurn(PlayerId);
                }
                else
                {
                    GD.Print("You Can't Claim Turn");
                }
            }
        }

        public async Task SkipTurnRequest()
        {
            GD.Print("SkipTurnFunc Func");
            var graphQlEndpoint = new Uri(URL);
            var graphQLClient = new GraphQLHttpClient(graphQlEndpoint, new NewtonsoftJsonSerializer());
            graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + AuthToken);
            var graphQLRequest = new GraphQLRequest
            {
                Query = @"mutation {
                    handOverTurn
                }"
            };

            GD.Print("Trying to send handOverTurn Mutation");

            var mutationResponse = await graphQLClient.SendMutationAsync<SkipTurnResponse>(graphQLRequest);

            GD.Print("Checking handOverTurn response");

            if (mutationResponse.Errors != null)
            {
                GD.Print($"GraphQL request failed: {mutationResponse.Errors[0].Message}");
            }
            else
            {
                GD.Print("Skip TURN  )))");
                GD.Print("Skip TURN  )))" + mutationResponse);
                if (mutationResponse.Data.Data.SkipTurn)
                {
                    GD.Print("Turn Skipped");
                    // GridManager.GM.TM.SkipUnitTurn();
                }
                else
                {
                    GD.Print("You Can't Skip Turn");
                }
            }
        }

        public async Task MoveUnit(UnitDataModel[] units)
        {
            //Get DogDataModel
            List<MoveDogInput> dogs = new List<MoveDogInput>();
            foreach (var d in units)
            {
                dogs.Add(new MoveDogInput(d.DogId, d.Tile, d.Health));
            }

            GD.Print("MoveUnit Mutation");

            var graphQlEndpoint = new Uri(URL);
            var graphQLClient = new GraphQLHttpClient(graphQlEndpoint, new NewtonsoftJsonSerializer());
            graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + AuthToken);
            var graphQLRequest = new GraphQLRequest
            {
                Query = @"
                    mutation ($moves: [MoveDogInput!]!) {
                        makeMove(moves: $moves)
                }",
                Variables = new {
                    moves = dogs.ToArray()
                }
            };

            GD.Print("Trying to send makeMove Query");

            var mutationResponse = await graphQLClient.SendMutationAsync<object>(graphQLRequest);

            if (mutationResponse.Errors != null)
            {
                GD.Print($"Move Unit request failed: {mutationResponse.Errors[0].Message}");
            }
            else
            {
                GD.Print("mutationResponse.Data = " + mutationResponse.Data.ToString());
            }
        }

        public async Task Attack(AppliedCardInput appliedCardInput, MoveDogInput[] opponantMoves, MoveDogInput[] ourMoves)
        {

            var graphQlEndpoint = new Uri(URL);
            var graphQLClient = new GraphQLHttpClient(graphQlEndpoint, new NewtonsoftJsonSerializer());
            graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + AuthToken);
            var graphQLRequest = new GraphQLRequest
            {
                Query = @"
                    mutation ($appliedAttackCard:AppliedCardInput!,$opponentMoves:[MoveDogInput!],$moves:[MoveDogInput!]!) {
                        makeAttack(appliedAttackCard:$appliedAttackCard,opponentMoves:$opponentMoves,moves:$moves)
                }",
                Variables = new {
                    appliedAttackCard = appliedCardInput,
                    opponentMoves = opponantMoves,
                    moves = ourMoves
                }
            };

            GD.Print("Trying to send Attack Query");

            var mutationResponse = await graphQLClient.SendMutationAsync<object>(graphQLRequest);

            if (mutationResponse.Errors != null)
            {
                GD.Print($"Attack Unit request failed: {mutationResponse.Errors[0].Message}");
            }
            else
            {
                GD.Print("Attack mutationResponse.Data = " + mutationResponse.Data.ToString());
            }
        }
    }
}
