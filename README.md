# Triskele.Captcha
This is a CAPTCHA generator based on .Net Core and completely loose coupled generator and repositories.
There is 3 types of generators (classes that create an image that consists captcha) and 2 repositories (the way you store captcha for later validation) implemented (in memory and Redis). 
## Creating a new generator
For this purpose you should create a class that inherites from "ICaptchaGenerator" interface and add the new class to appsettings json file in "GeneratorType".
## Creating a new repository
Repositories used for storing generated captcha and validating it.
For this purpose you should create a class that inherites from "ICaptchaRepository" interface and add the new class to appsettings json file in "RepositoryType".
