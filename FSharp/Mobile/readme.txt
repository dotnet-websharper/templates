To create a Html-based mobile app using Apache Cordova:

1. Replace the url to the deploy url in "Client.fs" in this project.
2. Install Tools for Apache Cordova with the Visual Studio installer if you don't have it already
3. Create a "Javascript > Blank App (Apache Cordova)" project in this solution named "$safeprojectname$.App"
    * If you name the Cordova project anything else, modify the CordovaAppPath property in "$safeprojectname$.fsproj"
4. Build and start this project then deploy the Cordova project
