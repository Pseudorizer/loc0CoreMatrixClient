# loc0CoreMatrixClient
A Lightweight Matrix Client Built With .Net Core

This is a lightweight client for interacting with the Matrix API. It can be used to create simple bots that recieved and send text/formatted messages, images, accept invites etc. I made this for personal use after I had to port a Discord bot of mine over to Riot. Also still a pretty newb programmer so if you want to improve this anyway feel free.

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
#### **Usage:** `MatrixCredentials matrixCredentials = new MatrixCredentials();`

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
#### **Usage:** `MatrixRoom room = new MatrixRoom(string:roomId, string:roomAlias);`

#### Methods
`MatrixRoom(string:roomId, string:roomAlias)`

Can take either a roomId or roomAlias

---
`SendText(MatrixTextMessage:textMessage, string:hostServer, string:accessToken):bool`

Sends a text message via a MatrixTextMessage object to the room, can be formatted or plain

Returns a bool to indicate success

---
`SendImage(string:matrixFileUrl, string:hostServer, string:accessToken):bool`

Sends an image via a mxc url to the room

Returns a bool to indicate success

---
`SendAudio(string:matrixFileUrl, string:hostServer, string:accessToken):bool`

Sends an audio file via a mxc url to the room

Returns a bool to indicate success

---
`SendVideo(string:matrixFileUrl, string:hostServer, string:accessToken):bool`

Sends a video file via a mxc url to the room

Returns a bool to indicate success

---
`SendFile(string:matrixFileUrl, string:hostServer, string:accessToken):bool`

Sends an generic file via a mxc url to the room

Returns a bool to indicate success

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
