 
 
/// TODO: Create integration tests for the plugin service and remove this project

This Application was writen as a helper for developing the actual Plugin service and should be refactored as integration test in the future.

In order to use the sample client you need to follow these instructions:

1-	Configure the TWCustomsPluginService by editing Application.Properties File in TWCustomsService deployed folder and configure the receivelocationurl field to refer to the url of port 9000 on the machine that will run the client.
      pluginservice.receivelocationurl=http://sydco-whza-1:9000/MockService/PostMessage
2-	Run the TWCPluginService.
3-	Run the sample client

In this sample client you can only attach a single file.


