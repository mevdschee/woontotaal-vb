# woontotaal-vb

To install dependencies run:

    dotnet restore

Then to compile and run:

    dotnet run

Or to just build:

    dotnet build

Enjoy!

### Install .net Core 2.2

I did the following:

    wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    sudo dpkg -i packages-microsoft-prod.deb
    sudo apt-get update
    sudo apt-get install apt-transport-https
    sudo apt-get install dotnet-sdk-2.2
    echo "export DOTNET_CLI_TELEMETRY_OPTOUT=1" >> ~/.bashrc

I run Ubuntu 18.04