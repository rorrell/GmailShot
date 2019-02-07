# Gmail Shot

This will log you into Gmail and then take a screenshot of your inbox and save it to your temp path (this was an interview project).

## Setup

Add your Google username and pass to the App.config appsettings.  Use https://coderstoolbox.net/string/#!encoding=xml&action=encode&charset=us_ascii or equivalent to xml encode the password if it has extra-special characters :)

### Limitations

You must handle your own 2FA, if you have that enabled.  It will only enter your username and password for you.