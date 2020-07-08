# Discord-Bot
A Discord C#/.NET Bot 

This Discord bot is a chat and voice coded in C#. This bot currently is still in development, further changes to come.

This bot is built to be run on any .NET Framework compatible machine. It is meant for single server usage due to the heavy amount of resources the voice part consumes.

## To Install:

To install do the following:
* Clone our repo
* Compile with Visual Studio, and it should be ready to go[prefix]
* Modify *config.json* with your bot token and other settings.
  * Move this file to the same folder as your executable.

## Current Features:

### Admin Functions:

[prefix]prefix [new prefix]
(This allows admins to change the command prefix.)

[prefix]mute [user]
(This allows admins to mute users.)

[prefix]unmute [user]
(This allows admins to unmute users.)

[prefix]kick [user] [reason]
(This allows admins to kick users.)

[prefix]ban [user] [reason]
(This allows admins to ban users.)

[prefix]addrole [user]
(This allows admins to add specific roles to a user.)

[prefix]delrole [user]
(This allows admins to remove specific roles to a user.)

### Music Functions:

[prefix]join
(Joins the user's voice channel.)

[prefix]leave
(Leaves the current voice channel.)

[prefix]play [url/index]
(Plays a song by url or local path.)

[prefix]pause
(Pauses the current song, if playing.)

[prefix]resume
(Pauses the current song, if paused.)

[prefix]stop
(Stops the current song, if playing or paused.)

[prefix]volume [num]
(Changes the volume to [0.0, 1.0].)

[prefix]add [url/index]
(Adds a song by url or local path to the playlist.)

[prefix]skip
(Skips the current song, if playing from the playlist.)

[prefix]playlist
(Shows what's currently in the playlist.)

[prefix]autoplay [enable]
(Starts the autoplay service on the current playlist.)

[prefix]download
(Download songs into our local folder.)

[prefix]songs
(Shows what's currently in our local folder.)

[prefix]cleanupsongs
(Cleans the local folder of duplicate files.)

### Chat Functions:

[prefix]botstatus [status]
(Allows admins to set the bot's current game to [status])

[prefix]say [msg]
(The bot will respond in the same channel with the message said.)

[prefix]clear [num]
(Allows admins to clear [num] amount of messages from current channel)

