# Falcons Roost 
 Falcons Roost is a discord chat bot. It interfaces with most of OpenAI's services. It also has a little game. It's very early in development.

## Planned features:
1. Platform agnostic (enable Twitch IRC, Mastodon, Slack, Teams and Telegram platforms)
2. AI generated combat messages.
3. "Card" awareness (In discord that looks like buttons when interacting with the bot)

## Required Tokens/API Keys
1. Get a Discord Token [here](https://discord.com/developers/applications).
2. Get an OpenAPI Key [here](https://platform.openai.com/account/api-keys)

## Local (Windows) use: 
Start the bot from the commandline and pass it a Discord Token and an Open AI API key. 
The command should look like this: `.\FalconsRoost.exe --dt <DiscordToken> --oa <OpenAIAPIKey>`

## Developer Environment:
To save yourself some headache, you can add User Secrets for the Discord Token and Open AI API key. Follow the instructions [here](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-7.0&tabs=windows) - specifically under the "Enable Secret Storage" and "Set a Secret" headings. The secrets are expected to be named "DiscordToken" and "OpenAI". 

## Docker Installation

Download and install [Docker Engine](https://docs.docker.com/engine/) and [Docker Compose](https://docs.docker.com/compose/).

Copy the following into a file named `docker-compose.yml`
``` 
services:
    mysql:
        image: mysql:8.0
        volumes: 
            - falcons-roost-volume:/var/lib/mysql
        environment:
            MYSQL_ROOT_PASSWORD: YourSQLPassword
            MYSQL_DATABASE: falconsroostdb
    falconsroost:
        image: billstrait/falconsroost:latest
        command: "trace dt=YourDiscordToken oa=YourOpenAIKey sqlpassword=YourSQLPassword"
volumes:
    falcons-roost-volume:
```

Change `YourSQLPassword` your password. It should be the same in both the `mysql` and `falconsroost` sections. You will also want to update `YourDiscordToken` and `YourOpenAPIKey` to match the appropriate values. 
Run the command `docker compose -f .\docker-compose.yml up`
