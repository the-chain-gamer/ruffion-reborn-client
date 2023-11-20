using Godot;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RuffGdMainProject.GameScene;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RuffGdMainProject.DataClasses
{
    public class SubscriptionController : Node2D
    {
        private ClientWebSocket _webSocketClient;
        private CancellationTokenSource _cancellationTokenSource;

        // Event to raise when a new client has connected
        public event Action<string> ClientConnected;

        public override void _Ready()
        {
            _webSocketClient = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();

            // ConnectWebSocket().Dispose();
        }

        public override async void _ExitTree()
        {
            await DisconnectWebSocket();
        }

        public async Task ConnectWebSocket(string AuthToken)
        {
            try
            {
                await _webSocketClient.ConnectAsync(new Uri("ws://localhost:3000/graphql"), _cancellationTokenSource.Token);

                await SendSubscriptionRequest(AuthToken);

                await ReceiveWebSocketData();
            }
            catch (Exception ex)
            {
                GD.Print("WebSocket connection error: " + ex.Message);
            }
        }

        private async Task DisconnectWebSocket()
        {
            try
            {
                _cancellationTokenSource.Cancel();

                if (_webSocketClient.State == WebSocketState.Open)
                    await _webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed by client", CancellationToken.None);

                _webSocketClient.Dispose();
            }
            catch (Exception ex)
            {
                GD.Print("WebSocket disconnection error: " + ex.Message);
            }
        }

        private async Task SendSubscriptionRequest(string AuthToken)
        {
            var options = new GraphQLHttpClientOptions
            {
                //insert server url here...
                EndPoint = new Uri("ws://localhost:3000/graphql"),
                ConfigureWebSocketConnectionInitPayload = (opt) =>
                {
                    var payload = new
                    {
                        auth_token = AuthToken
                    };
                    return payload;
                }
            };

            var graphQLClient = new GraphQLHttpClient(options, new NewtonsoftJsonSerializer());
            var userJoinedRequest = new GraphQLRequest
            {
                Query = @"
                subscription {
                    gameUpdates {
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
                        appliedAttackCard {
                            ...AppliedCard
                        }
                        eventName
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
                fragment AppliedCard on AppliedCard{
                    dogId
                    cardId
                    targetDogs {
                        ...TargetDogs
                    }
                }
                fragment TargetDogs on TargetDogs {
                    playerId
                    dogId
                }
                "
            };

            IObservable<GraphQLResponse<SubscriptionData>> subscriptionStream
            = graphQLClient.CreateSubscriptionStream<SubscriptionData>(userJoinedRequest);

            subscriptionStream = graphQLClient.CreateSubscriptionStream<SubscriptionData>(userJoinedRequest);

            // var subscription = subscriptionStream.Subscribe(response => Console.WriteLine($"user '{response.Data }' joined"));

            subscriptionStream.Subscribe(
                (response) =>
                {
                    // Get the user id from the response
                    var multiplayerMngr = StartupScript.Startup.GetChild<MultiplayerManager>(0);
                    multiplayerMngr.RecieveSubscriptionData(response.Data);
                },
                (error) =>
                {
                    // Handle the subscription error.
                    GD.Print("Error '{ error }'");
                },
                () =>
                {
                    // The subscription has been closed.
                    GD.Print("Connection Closed");
                }
            );
        }

        private async Task ReceiveWebSocketData()
        {
            var buffer = new byte[4096];

            while (_webSocketClient.State == WebSocketState.Open)
            {
                var receiveResult = await _webSocketClient.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (receiveResult.MessageType == WebSocketMessageType.Text)
                {
                    var data = System.Text.Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                    var response = JObject.Parse(data);

                    // Check if it is a client connection message
                    if (response.ContainsKey("type") && response["type"].ToString() == "clientConnected")
                    {
                        var clientId = response["clientId"].ToString();

                        // Raise the ClientConnected event with the client ID
                        ClientConnected?.Invoke(clientId);
                    }
                }
            }
        }
    }
}