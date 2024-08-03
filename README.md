# MobileBgWatch
This is a work-in-progress web app that allows you to easily monitor specific searches from mobile.bg

# A quick guide:

1. Go to <a href="https://www.mobile.bg/" target="_blank">Mobile.bg</a> and select your desired filters. After that, click on Search.

![image](https://github.com/user-attachments/assets/061bcf57-7289-4fb5-8901-423bf05de190)

2. Copy the generated link, paste it here and click Add. During the police car gif, the program is going through all pages of the search link and getting all vehicles.

![img-1](https://github.com/user-attachments/assets/0c982c89-7d20-421c-865c-153cd951ae7e)

3. If everything went well, you should see the first 40 vehicles on the front page for each search link.

![img-2](https://github.com/user-attachments/assets/28391e1a-5e58-4e8d-a524-ee0f816ebcd7)

4. You can refresh a search link to get new vehicle ads and update the current ones in the database.

5. When you open the vehicle page, you can see all the relevant information, including price history.  

![img-3](https://github.com/user-attachments/assets/8155b623-d591-4d34-96e4-fa5213a8373c)

There is also a Favorites page where you can add or remove vehicles by pressing the heart icon.

![img-4](https://github.com/user-attachments/assets/060815a7-ed73-4077-ab06-24b8e87df2c2)

The rest of the options you can check out for yourself. I've also implemented a background service to update the database for each user search link automatically. Every 15 minutes, it does a small search and stops after finding a matching vehicle ad, and every hour it performs a full search to update the database with new ads, changed prices and images.

I have more ideas, and one of them is to implement SendGrid to email users when there are new vehicle ads.

This repository is licensed under the Creative Commons Attribution-NonCommercial 4.0 International Public License. See the [LICENSE](./LICENSE) file for details.
