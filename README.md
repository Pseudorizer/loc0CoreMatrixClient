# loc0CoreMatrixClient
A Limited Matrix Client Built With .Net Core

This is a limited client for interacting with the Matrix API. It can be used to create simple bots that recieved and send text/formatted messages, images, accept invites etc. I made this for personal use after I had to port a Discord bot of mine over to Riot. Also, still a pretty newb programmer so if you want to improve this in anyway feel free.

# Current Progress
* Login with username/password
* Join Rooms
* Figure out syncing
* Create a listener for messages
* Create a class for interacting with rooms
* Upload content
* Allow joining rooms from invites

# Todo
* Allow encryption to be used - Biggest task
* Whatever the future tells me to do!

# Example Code
```
private static readonly MatrixClient MatrixClient = new MatrixClient();

public static void Main()
{
    Example().GetAwaiter().GetResult();
}

private static async Task Example()
{
    MatrixCredentials matrixCredentials = new MatrixCredentials("Username", "Password", "DeviceID" " DeviceName");

    Console.WriteLine("Logging in...");

    if (await MatrixClient.Login("hostServer", matrixCredentials))
    {
        Console.WriteLine("Success");

        List<string> roomsToJoin = new List<string>
        {
            "#room1:host",
            "!IDOfRoom:host"
        };

        List<string> joinResults = await MatrixClient.JoinRooms(roomsToJoin, true);

        foreach (var joinResult in joinResults)
        {
            Console.WriteLine(joinResult);
        }

        MatrixClient.MessageReceived += MessageReceivedHandlerExample;

        MatrixClient.StartListener();

        await Task.Delay(-1);
    }
    else
    {
        Console.WriteLine("Failed");
    }
}

private static async void MessageReceivedHandlerExample(MessageReceivedEventArgs args)
{
    MatrixRoom messageRoom = MatrixClient.GetMatrixRoomObject(args.RoomId);

    if (args.SenderId == "a" && args.Message.StartsWith("ping"))
    {
        MatrixTextMessage message = new MatrixTextMessage
        {
            Body = "pong"
        };

        var sendResult = await messageRoom.SendMessage(message);
    }
}
```

# Documentation

#### Format: method(type:parameterName):returnType

### MatrixClient
#### **Usage:** `MatrixClient matrixClient = new MatrixClient();`

#### Methods
`MatrixClient(int:messageLimit=10)`

messageLimit is optional and defaults to 10

---
`Login(string:hostServer, MatrixCredentials:credentials):bool`

Login to a Matrix account through a MatrixCredentials object

Returns bool to indicate success

---
`Logout():bool`

Logout of the Matrix account currently in use

Will reset all properties of MatrixClient

---
`JoinRooms(List<string>:roomsToJoin, bool:retryFailure):List<string>`

Takes a list of rooms to join; retry failure will auto-retry any failed connects

Returns a list of strings containing success/failure messages

---
`JoinRoom(string:roomsToJoin, bool:retryFailure):string`

Takes a single room to join

Returns a single string containing a success/failure message

---
`GetMatrixRoomObject(string:roomId):MatrixRoom`

Accesses a dictionary which contains MatrixRoom objects for each room you've joined

Room ID acts as the key

I'd recommend you use this to create a new MatrixRoom instance though it's not required

---
`StartListener():void`

Starts a listener task for events in any rooms you've joined

---
`StopListener():void`

Cancels the listener task

---
`Upload(string:filePath):string`

Uploads any file to Matrix and returns the mxc url

---
#### Properties
`string:AccessToken`

Your access token

---
`string:DeviceId`

The device ID in use

---
`string:HomeServer`

The homeserver being used by the account

---
`string:UserId`

The user ID in use

---
`string:FilterId`

Id for filter being used in syncing

---
#### Events
`MessageReceived:MessageReceivedEventArgs`

An event for recieved messages

Passes a MessageReceivedEventArgs to your handler

---
`InviteReceived:InviteReceivedEventArgs`

An event for recieved invites

Passes a InviteReceivedEventArgs to your handler

---
### MatrixCredentials
#### **Usage:** `MatrixCredentials matrixCredentials = new MatrixCredentials(string:Username, string:Password, string:DeviceID string:DeviceName);`

#### Properties
`string:Username`

Username of account

---
`string:DeviceName`

Device name client should use, if one is not supplied it will be auto-generated

---
`string:Password`

Password of account

---
`string:DeviceId`

Device ID client should use, if one is not supplied it will be auto-generated

---
### MatrixRoom
#### **Usage:** `MatrixRoom room = new MatrixRoom(string:homeServer, string:accessToken, string:roomId, string:roomAlias);`

#### Methods
`MatrixRoom(string:homeServer, string:accessToken, string:roomId, string:roomAlias)`

Can take either a roomId or roomAlias

---
`SendMessage(MatrixTextMessage:textMessage):bool`

Sends a text message to the room, can be either a simple plain text message or HTML formatted

Returns a bool to indiciate success

---
`SendMessage(MatrixFileMessage:fileMessage):bool`

Sends an uploaded file to the room via a MxcUrl

Returns a bool to indiciate success

---
#### Properties
`string:RoomId`

roomId in use by the MatrixRoom instance

---
`string:RoomAlias`

roomAlias in use by the MatrixRoom instance

---
### MatrixTextMessage
#### **Usage:** `MatrixTextMessage matrixTextMessage = new MatrixTextMessage();`

#### Properties
`string:Body`

Plain text body of the message

---
`string:FormattedBody`

HTML formatted text body of the message, you do not need both, only one or the other

---
`string:Format`

Format of the message, should be left empty unless the message is formatted, will be auto changed anyway

---
`string:MsgType`

MsgType, should always be m.text

---
### MatrixFileMessage
#### **Usage:** `MatrixFileMessage matrixFileMessage = new MatrixFileMessage();`

#### Properties
`string:Filename`

Filename to be used when sending the file, this does not change the filename if someone downloads the file

---
`string:Description`

Description to be used for the file, defaults to the filename

---
`string:Type`

Type of the message being sent I.E. m.image, m.file etc.

---
`string:MxcUrl`

Mxc url of the uploaded file
