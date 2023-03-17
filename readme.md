Falcons Roost is a discord chat bot. It interfaces with most of OpenAI's services. It also has a little game. It's very early in development.

Planned features:
1. Platform agnostic (enable Twitch IRC, Mastodon, Slack, Teams and Telegram platforms)
2. AI generated combat messages.
3. "Card" awareness (In discord that looks like buttons when interacting with the bot)

Local use: 
Start the bot from the commandline and pass it a Discord Token (obtained from [here](https://discord.com/developers/applications)) and an Open AI API key (obtained from (here)[https://platform.openai.com/account/api-keys]). 
The command should look like this: `.\FalconsRoost.exe --dt <DiscordToken> --oa <OpenAIAPIKey>`

Developer Environment:
To save yourself some headache, you can add User Secrets for the Discord Token and Open AI API key. Follow the instructions [here](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-7.0&tabs=windows) - specifically under the "Enable Secret Storage" and "Set a Secret" headings. The secrets are expected to be named "DiscordToken" and "OpenAI". 

