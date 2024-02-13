# SocialMediaApp
## Description
A basic social media application for my assignement. It contains multiple authentication methods and also has the ability to send emails. The project uses ASP.NET Core Entity Framework as its ORM for the authentication and the dataaccess(2 databases one for authentication and one for authorization).

## Modules Of Application
The application consists of 4 modules as of now
1. SocialMediaApp.MVC - ASP.NET CORE MVC Acts As FrontEnd(targets .NET 7) - references every other module
2. SocialMediaApp.SharedModels - Class Library that contains shared models that are used throughout the app to avoid copying/cloning(probably lazy solution targets .NET 7) - references nothing
3. SocialMediaApp.EmailServiceLibrary - Class Library that contains logic for sending emails(targets .NET 7) - references nothing
4. SocialMediaApp.AuthenticationLibrary - Class Library that contains logic for authentication(targets .NET 7) - references SocialMediaApp.SharedModels

## SocialMediaApp.EmailServiceLibrary
The library exposes the following methods through the IAuthenticationProcedures Interface:
1. SendContactFormEmailAsync used to send emails through a contact form
    ```csharp 
    Task<bool> SendContactFormEmailAsync(string emailSender, string title, string body);
    ```
2. SendEmailAsync used to send regular emails
    use the following as the default way of sending email(it uses the default email to send email)
    ```csharp
    Task<bool> SendEmailAsync(string emailReceiver, string title, string body);
    ```
    or use the following to add your own email, which you will have to configure, to send emails
    ```csharp
    Task<bool> SendEmailAsync(string emailSender, string emailReceiver, string title, string body);
    ```

## SocialMediaApp.AuthenticationLibrary
### User Account CRUD
#### Create User Account Methods
1. RegisterUserAsync is used to create a user account. It returns the id of the newly created account throught the first return value and it also returns a confirmation token through the second return value. This method does not activate the user account it only creates the account in the database, which can be activated through the confirmation token.
    ```csharp
    Task<(string, string)> RegisterUserAsync(AppUser appUser, string password, bool isPersistent);
    ```
2. ConfirmEmailAsync is used to activate a user account. If the userId or the userId-confirmationToken combination is invalid the method fails. The method returns a boolean value, which is the success status of the method.
    ```csharp
    Task<bool> ConfirmEmailAsync(string userId, string confirmationToken);
    ```

#### Update And Reset User Account Methods
1. UpdateUserAccountAsync is used to update the user based on the userId field of the appUser, which can not be blank. This method does not update the password of the account and should not be used in general to update the email of the account. The method returns a boolean value, which is the success state of the method.
    ```csharp
    Task<bool> UpdateUserAccountAsync(AppUser appUser);
    ```

2. ChangePasswordAsync is used to change the password of a user account based on the Id field of the appUser. If the currentPassword is invalid for the given account then the method fails. The method returns a boolean value, which is the success status of the method, and it also returns a string value, which is used to discern the reason as to why the method failed, for example it could be that the currentPassword is invalied. 
    ```csharp
    Task<(bool, string)> ChangePasswordAsync(AppUser appUser, string currentPassword, string newPassword);
    ```
3. CreateChangeEmailTokenAsync is used to create an email reset token for the given account(based on the id field of the appUser). The method returns that reset token, which can be used to activate the new email email of the account.
    ```csharp
    Task<string> CreateChangeEmailTokenAsync(AppUser appUser, string newEmail);
    ```
4. ChangeEmailAsync is used to change the email of a user account to the email contained in the newEmail parameter. If the userId or the userId-changeEmailToken is invalid the method fails. The method returns a boolean value, which is the success status of the method.  
    ```csharp
    Task<bool> ChangeEmailAsync(string userId, string changeEmailToken, string newEmail);
    ```

5. CreateResetPasswordTokenAsync is used to create a password reset token for the given account(based on the id field of the appUser), which can be useful if the user forgets their password. The method returns that reset token, which can be used to reset password of the user. 
    ```csharp
    Task<string> CreateResetPasswordTokenAsync(AppUser appUser);
    ```
6. ResetPasswordAsync is used to reset the password of user account. If the resetPassword is not token or the userId is invalid the method fails. The method returns a boolean value, which is the success state of the method.
    ```csharp
    Task<bool> ResetPasswordAsync(string userId, string resetPasswordToken, string newPassword);
    ```         

#### Delete User Account Methods
1. DeleteUserAccountAsync is used to delete a user account(based on the id field of the appUser). The method returns a boolean value, which is the success status of the method. 
    ```csharp
    Task<bool> DeleteUserAccountAsync(AppUser appUser);
    ```

#### Retrieve User Account Methods
1. FindByEmailAsync is used to return a user account(type AppUser) based on the given email. This method only populates the fields of the base class IdentityUser and does not populate the extra fields that the AppUser class might have.
    ```csharp
    Task<AppUser> FindByEmailAsync(string email);
    ```
2. FindByUserIdAsync is used to return a user account(type AppUser) based on the given user id. This method only populates the fields of the base class IdentityUser and does not populate the extra fields that the AppUser class might have.
    ```csharp
    Task<AppUser> FindByUserIdAsync(string userId);
    ```
3. FindByUsernameAsync is used to return a user account(type AppUser) based on the given username. This method only populates the fields of the base class IdentityUser and does not populate the extra fields that the AppUser class might have.
    ```csharp
    Task<AppUser> FindByUsernameAsync(string username);
    ```
4. GetUsersAsync is used to return a list of the users(both confirmed accounts and unconfirmed) of class AppUser from the application's database. The extra  fields of the class AppUser for each element of the list are not populated through this method. 
    ```csharp
    Task<List<AppUser>> GetUsersAsync();
    ```

### User Account Session State
1. CheckIfUserIsLoggedIn is used to check if the user of the current session is logged in(through cookies). The method returns a boolean value, which is the success status of the method. 
    ```csharp
    Task<bool> CheckIfUserIsLoggedIn();
    ```
2. GetCurrentUserAsync is used to retrieve the current signed in user of the given session from the database. It does not return on its own the extra fields that AppUser might contain and are not contained in the base class Identity User
    ```csharp 
    Task<AppUser> GetCurrentUserAsync(); 
    ```
3. SignInUserAsync is used to sign in the user persistenly or not persistenly based on the isPersistent parameter. If the username is invalid or the combination of the credentials is invalid the method fails. The method returns a boolean value, which is the success state of the method.
    ```csharp
    Task<bool> SignInUserAsync(string username, string password, bool isPersistent);
    ```
4. LogOutUserAsync is simply used to log out the user.
    ```csharp
    Task LogOutUserAsync();
    ```





