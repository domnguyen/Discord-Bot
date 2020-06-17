# Discord-Bot
A Discord C#/.NET Bot 

This Discord bot is a chat and voice coded in C#. This bot currently is still in development, further changes to come.

This bot is built to be run on any .NET Framework compatible machine. It is meant for single server usage due to the heavy amount of resources the voice part consumes.

## To Install:

To install do the following:
* Clone our repo
* ~Create a file called BotToken.txt in the /src directory with your bot token to get connected.~
  * ~The file should contain only the token, nothing else.~
  * [06/16] This will be moved to a config file.
* Compile with Visual Studio, and it should be ready to go!

## Current Features:

### Admin Functions:

!mute [user]
(This allows admins to mute users.)

!unmute [user]
(This allows admins to unmute users.)

!kick [user] [reason]
(This allows admins to kick users.)

!ban [user] [reason]
(This allows admins to ban users.)

!addrole [user]
(This allows admins to add specific roles to a user.)

!delrole [user]
(This allows admins to remove specific roles to a user.)

### Music Functions:

!join
(Joins the user's voice channel.)

!leave
(Leaves the current voice channel.)

!play [url/index]
(Plays a song by url or local path.)

!pause
(Pauses the current song, if playing.)

!resume
(Pauses the current song, if paused.)

!stop
(Stops the current song, if playing or paused.)

!volume [num]
(Changes the volume to [0.0, 1.0].)

!add [url/index]
(Adds a song by url or local path to the playlist.)

!skip
(Skips the current song, if playing from the playlist.)

!playlist
(Shows what's currently in the playlist.)

!autoplay [enable]
(Starts the autoplay service on the current playlist.)

!download
(Download songs into our local folder.)

!songs
(Shows what's currently in our local folder.)

!cleanupsongs
(Cleans the local folder of duplicate files.)

### Chat Functions:

!botstatus [status]
(Allows admins to set the bot's current game to [status])

!say [msg]
(The bot will respond in the same channel with the message said.)

!clear [num]
(Allows admins to clear [num] amount of messages from current channel)

