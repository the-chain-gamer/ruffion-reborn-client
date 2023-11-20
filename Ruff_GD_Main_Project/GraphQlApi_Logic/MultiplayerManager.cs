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
        //a reference to subscription controller
        private SubscriptionController subscriptionController;
        //list that holds records of available rooms
        private List<MatchDataModel> availableRooms;
        //joined room data model
        private MatchDataModel JoinedRoom;
        //Put here the actual server address when publishing
        private const string URL = "http://localhost:3000/graphql";
        //Auth Token provided by server.
        private string AuthToken = "";
        private bool isGameStarted = false;

        //GraphQL http client options object
        private GraphQLHttpClientOptions graphQLHttpClientOptions = new GraphQLHttpClientOptions {
            EndPoint = new Uri(URL)
        };

        public GraphQLHttpClientOptions GraphQLHttpClientOptions { get => graphQLHttpClientOptions; set => graphQLHttpClientOptions = value; }

        //Player ID
        public string PlayerId { get; private set; }
        
        public override void _Ready()
        {
            subscriptionController = GetNode<SubscriptionController>("SubscriptionManager");
            //registering the event
            subscriptionController.ClientConnected += OnClientConnected;
        }

        //On client connected event handler
        private void OnClientConnected(string clientId)
        {
            GD.Print("New client connected:", clientId);
            // Handle the new client connection event
        }

        //Searching for rooms
        public async Task SearchForRooms(UserDataModel playerData)
        {
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

            if (availableRooms.Count > 0)
            {
                var rooms = availableRooms.FindAll(x => x.Status == "WAITING");
                if (rooms.Count > 0)
                {
                    var random = new RandomNumberGenerator();
                    random.Randomize();
                    var roomNum = random.RandiRange(1, rooms.Count);
                    var room = rooms[roomNum - 1];
                    var joinedRoom = JoinRoom(playerData, room);
                }
                else
                {
                    var createdRoom = CreateRoom(playerData);
                }
            }
            else
            {
                var createdRoom = CreateRoom(playerData);
            }
        }
        public async Task JoinRoom(UserDataModel playerData, MatchDataModel room)
        {
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

            var mutationResponse = await graphQLClient.SendMutationAsync<JoinGameResponse>(graphQLRequest);

            if (mutationResponse.Errors != null)
            {
                GD.Print($"Join Game GraphQL request failed: {mutationResponse.Errors[0].Message}");
            }
            else
            {
                AuthToken = mutationResponse.Data.ResponseData.AuthToken.Token;
                JoinedRoom = mutationResponse.Data.ResponseData.Game;
                PlayerId = playerData.UserId;
                Logger.UiLogger.Log(Logger.LogLevel.INFO, "Joined room & looking for subs");
                if(mutationResponse.Data.ResponseData.Game.Status.Equals("STARTED") || mutationResponse.Data.ResponseData.Game.EventName.Equals("JOINED"))
                {
                    StartupScript.Startup.LoadOpponentImgs(mutationResponse.Data.ResponseData.Game.Player1, mutationResponse.Data.ResponseData.Game.Player2);
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

            var mutationResponse = await graphQLClient.SendMutationAsync<Data>(graphQLRequest);

            if (mutationResponse.Errors != null)
            {
                GD.Print($"GraphQL request failed: {mutationResponse.Errors[0].Message}");
            }
            else
            {
                if (mutationResponse.Data != null)
                {
                    AuthToken = mutationResponse.Data.ResponseData.AuthToken.Token;
                    JoinedRoom = mutationResponse.Data.ResponseData.Game;
                    PlayerId = playerData.UserId;
                    SubscribeForEvents();
                }
                else
                {
                    GD.Print("NO SUCCESS");
                }
            }
        }

        public async void SubscribeForEvents()
        {
            var abc = subscriptionController.ConnectWebSocket(AuthToken);
        }

        public void RecieveSubscriptionData(SubscriptionData data)
        {
            Logger.UiLogger.Log(Logger.LogLevel.INFO, "In Subscription response");
            Logger.UiLogger.Log(Logger.LogLevel.INFO, "data.GameUpdates.Status = " + data.GameUpdates.Status);
            string eventName = data.GameUpdates.EventName.ToString();
            if(!isGameStarted && (data.GameUpdates.Status.Equals("STARTED") || eventName.Equals("JOINED")))
            {
                StartupScript.Startup.LoadOpponentImgs(data.GameUpdates.Player1, data.GameUpdates.Player2);
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
                string currentPlayerID = data.GameUpdates.CurrentPlayer.ID;
                var player = GridManager.GM.TM.GetPlayerByID(currentPlayerID);
                var totalTurns = data.GameUpdates.Player1.Turn + data.GameUpdates.Player2.Turn;
                GridManager.GM.TM.ChangeTurn(player, data.GameUpdates.CurrentPlayer.Turn, totalTurns);
            }
            else if(eventName.Equals("MOVE"))
            {
                GridManager.GM.MoveUnits(data.GameUpdates.CurrentPlayer);
            }
            else if(eventName.Equals("WINNER"))
            {
                Logger.UiLogger.Log(Logger.LogLevel.INFO, "The winnder is = " + data.GameUpdates.Winner.ID);
            }
            else if(eventName.Equals("FORFEIT"))
            {
                Logger.UiLogger.Log(Logger.LogLevel.INFO, "Other Player Disconnected");
                StartupScript.Startup.ReturnToMenu();
            }
            else if(eventName.Equals("ATTACK"))
            {
                Logger.UiLogger.Log(Logger.LogLevel.INFO, "Unit Attacked!!!");
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
            var graphQlEndpoint = new Uri(URL);
            var graphQLClient = new GraphQLHttpClient(graphQlEndpoint, new NewtonsoftJsonSerializer());
            graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + AuthToken);
            var graphQLRequest = new GraphQLRequest
            {
                Query = @"mutation {
                    claimTurn
                }"
            };


            var mutationResponse = await graphQLClient.SendMutationAsync<ClaimTurnResponse>(graphQLRequest);

            if (mutationResponse.Errors != null)
            {
                GD.Print($"GraphQL request failed: {mutationResponse.Errors[0].Message}");
            }
            else
            {
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
            var graphQlEndpoint = new Uri(URL);
            var graphQLClient = new GraphQLHttpClient(graphQlEndpoint, new NewtonsoftJsonSerializer());
            graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + AuthToken);
            var graphQLRequest = new GraphQLRequest
            {
                Query = @"mutation {
                    handOverTurn
                }"
            };


            var mutationResponse = await graphQLClient.SendMutationAsync<SkipTurnResponse>(graphQLRequest);

            if (mutationResponse.Errors != null)
            {
                GD.Print($"GraphQL request failed: {mutationResponse.Errors[0].Message}");
            }
            else
            {
                if (mutationResponse.Data.Data.SkipTurn)
                {
                    GD.Print("Turn Skipped");
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
