# Plaid quickstart for WPF

[![Build](https://github.com/jcoliz/PlaidQuickstart.WPF/actions/workflows/buildtest.yml/badge.svg)](https://github.com/jcoliz/PlaidQuickstart.WPF/actions/workflows/buildtest.yml)
[![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-2.1-4baaaa.svg)](code_of_conduct.md) 
[![Plaid API](https://img.shields.io/badge/Plaid%20API-v1.482.3-blue
)](https://github.com/plaid/plaid-openapi)

This repository is a port of the official [Plaid quickstart](https://github.com/plaid/quickstart) project, using the [Going.Plaid](https://github.com/viceroypenguin/Going.Plaid) client libraries, for the Windows Presentation Foundation (WPF) on .NET 8.0.

## 1. Clone the repository

```Powershell
PS> git clone https://github.com/jcoliz/PlaidQuickstart.WPF
PS> cd PlaidQuickstart.WPF
```

## 2. Set up your secrets

```Powershell
PS> cd .\FrontEnd\
PS FrontEnd> cp .\secrets.example.yaml .\secrets.yaml
```

Copy `secrets.example.yaml` to a new file called `secrets.yaml`, then fill out the configuration variables inside. At
minimum `ClientID` and `Secret` must be filled out. Get your Client ID and secrets from
the [Plaid dashboard](https://dashboard.plaid.com/account/keys)

> NOTE: The `secrets.yaml` files is included as a convenient local development tool. In fact, you can use any of the many methods of getting configuration settings into ASP.NET. Please see [Safe storage of app secrets in development in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=linux).

## 3. Install .NET 8.0 SDK and/or Visual Studio 2022 17.7 or later

If you don't already have the .NET 8.0 SDK installed, be sure to get a copy first from the [Download .NET](https://dotnet.microsoft.com/en-us/download) page.

As with all WPF apps, it's recommended to use Visual Studio for development. This project relies on the C# 12 compiler, which is available in versions 17.7 or later.

## 4. Run it!

Build and run the solution using Visual Studio. Or, from the command-line:

```Powershell
PS> dotnet build FrontEnd
PS> start .\FrontEnd\bin\x64\Debug\net8.0-windows\FrontEnd.exe
```

## Test credentials

In Sandbox, you can log in to any supported institution (except Capital One) using `user_good` as the username and `pass_good` as the password. If prompted to enter a 2-factor authentication code, enter `1234`.

In Development or Production, use real-life credentials.

## Code of conduct

We as members, contributors, and leaders pledge to make participation in our
community a harassment-free experience for everyone. We pledge to act and
interact in ways that contribute to an open, welcoming, diverse, inclusive, 
and healthy community.

Please review the [Code of conduct](/code_of_conduct.md) for more details.
