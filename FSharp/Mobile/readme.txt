To create a Html-based mobile app using Apache Cordova:

1. Install Tools for Apache Cordova with the Visual Studio installer if you don't have it already
2. Create a "Javascript > Blank App (Apache Cordova)" project in this solution named "$safeprojectname$.App"
    * If you name the Cordova project anything else, modify the CordovaAppPath property in "$safeprojectname$.fsproj"
3. Build this project then deploy the Cordova project

Using remoting:

1. If you want the mobile app to make server calls, create a "WebSharper Client-Server" project.
2. Reference that project in this one, and add a line under "let Main () =" in "Client.fs" here:
    WebSharper.Remoting.EndPoint <- "http://localhost:9000/" // replace URL to the address where the server is deployed
3. Now you can call RPC functions defined in your server project from your mobile app.