Falcons Roost is a discord chat bot. It interfaces with most of OpenAI's services. It also has a little game. It's very early in development.

Planned features:
1. Platform agnostic (enable Twitch IRC, Mastodon, Slack, Teams and Telegram platforms)
2. AI generated combat messages.
3. "Card" awareness (In discord that looks like buttons when interacting with the bot)

Local use: 
Start the bot from the commandline and pass it a Discord Token (obtained from [here](https://discord.com/developers/applications)) and an Open AI API key (obtained from [here](https://platform.openai.com/account/api-keys)). 
The command should look like this: `.\FalconsRoost.exe --dt <DiscordToken> --oa <OpenAIAPIKey>`

Developer Environment:
To save yourself some headache, you can add User Secrets for the Discord Token and Open AI API key. Follow the instructions [here](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-7.0&tabs=windows) - specifically under the "Enable Secret Storage" and "Set a Secret" headings. The secrets are expected to be named "DiscordToken" and "OpenAI". 

*Docker - RaspberryPi:*
The included dockerfile is targeting a Raspberry Pi 4 running Ubuntu 20.04. To use it...
1. Install docker on your raspberry pi - I followed the instructions found [here](https://phoenixnap.com/kb/docker-on-raspberry-pi).
2. Install docker on your development machine - I installed it on windows from [here](https://www.docker.com/).
3. Modify the docker file.
	a. Rename/Copy `dockerfile-Template` to `dockerfile.` It's a good idea to add dockerfile to your .gitignore list, as we're about to put some sensitive information in it.
	b. On line 20 you will find `CMD` - you will modify the arguements between the `[]`.
	c. "trace" can be removed if you want to disable tracing. It will add some useful, but not always relevant, information to your log.
	d. Change 'YourDiscordToken' to your Discord token. Leave the `dt=` in front of it.
	e. Change 'YourOpenAPIKey' to your OpenAI API key. Leave the `oa=` in front of it.
4. Open a command line/powershell/bash terminal in the project's root directory. (You're using [Windows Terminal](https://www.microsoft.com/store/productId/9N0DX20HK701) right?).
5. Run `docker build --no-cache -t falconsroost .` This will tell Docker to execute the instructions in the dockerfile to create an image for the application.
6. Run `docker save falconsroost:latest -o fr.tar` This will tell docker to export the image you created to a file called "fr.tar"
7. Copy the image from your development machine to your Raspberry Pi, however you choose to do that. I use the command `cp .\fr.tar \\pi\pishare\fr.tar` to accomplish this from PowerShell (I believe it'd work in bash too) where `\\ubuntu\pishare\` is the name of an smb mounted directory on the PI.
8. SSH to your Raspberri Pi, steps 9 and 10 happen there.
9. Run `sudo docker load -i /some/directory/fr.tar` where `/some/directory/` is where you put the tar file in step 7. This tells docker to load the image and make it available for use.
10. Enter `sudo docker run -d --name falconsroost --restart always falconsroost` - this tells Docker to run the image and make sure it is running whenever docker is.
