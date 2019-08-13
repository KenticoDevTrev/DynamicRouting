# DynamicRouting
Dynamic Routing in Kentico using a Routing Table and Assembly Attribute

# First time Setup Preperation for development

1. Include a Kentico SP (12.0.29) version of the Lib folder to the root of the solution, as the "Mother" Project references these
1. If there is a packages folder, delete it.
1. Open the DynamicRouting.sln at the root and rebuild, you may have to reinstall nuget packages.
1. Also open the DynamicRouting.Kentico/DynamicRouting.Kentico.sln solution and make sure it builds.
1. On your MVC site solution, include the DynamicRouting.MVC project and DynamicRouting.MVC.Views project.
1. On your MVC Site project, add a reference to the DynamicRouting.MVC Project, and a reference to the DynamicRouting.MVC.Views.dll in the bin folder of the DynamicRouting.MVC project

For the "Mother" side, you will need to import the latest export of the module (soon to come), and add a reference to the DynamicRouting.Kentico project in your Mother Web App.
