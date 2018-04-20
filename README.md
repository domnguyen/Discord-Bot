# Discord-Bot
A Discord C#/.NET Bot 

This Discord bot is a chat and voice coded in C#. This bot currently is still in development, further changes to come.

This bot is built to be run on any .NET Framework compatible machine. It is meant for single server usage due to the heavy amount of resources the voice part consumes.

## To Install:

To install do the following:
* Clone our repo
* Create a file called BotToken.txt in the /src directory with your bot token to get connected.
  * The file should contain only the token, nothing else.
* Compile with Visual Studio, and it should be ready to go!

## Current Features:

### Chat Functions:

!botstatus [status](Allows admins to set the bot's current game to [status])

!say [msg](The bot will respond in the same channel with the message said.)

!clear [num](Allows admins to clear [num] amount of messages from current channel)

!mute [user](This allows admins to mute users.)

!unmute [user](This allows admins to unmute users.)

### Music Function

!join (Joins the user's voice channel.)

!leave (Leaves the current voice channel.)

!play [url] (Plays a song by url or local path.)

!pause (Pauses the current song, if playing.)

!resume (Pauses the current song, if paused.)

!stop (Stops the current song, if playing or paused.)

!volume [num] (Changes the volume to [0.0, 1.0].)

!add [url] (Adds a song by url or local path to the playlist.)

!skip (Skips the current song, if playing from the playlist.)

!playlist (Shows what's currently in the playlist.)

!autoplay [enable](Starts the autoplay service on the current playlist.)
