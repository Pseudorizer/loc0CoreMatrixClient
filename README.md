# loc0NetMatrixClient
A Matrix Client Built With .Net Core [WIP]
If you find this right now, this is **very early** development. Very early in a sense that I started coding this one day before creating this repo. Also, this is probably the biggest thing I've coded in my just over a year of programming experience, so the code isn't 100% amazing.

# Current Progress
* Login with username/password
* Join Rooms
* Figure out syncing
* Create a listener for messages
* Create a class for interacting with rooms

# Todo
* Upload content
* Allow joining rooms from invites
* Allow encryption to be used
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
`MessageReceivedEventArgs:MessageReceived`

An event for recieved messages

Passes a MessageReceivedEventArgs to your handler
