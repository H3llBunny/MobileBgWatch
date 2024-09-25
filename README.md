<p align="center">
  <img src="https://github.com/user-attachments/assets/99a7c9c6-3cdc-427a-89e5-1dab834b3fb6" alt="cropforgithub" />
</p>
This is a work-in-progress web app that allows you to easily monitor specific searches from mobile.bg

# A quick guide:

1. Go to <a href="https://www.mobile.bg/search/avtomobili-dzhipove" target="_blank">Mobile.bg</a> and select your desired filters. After that, click on Search.

![guide-img-1](https://github.com/user-attachments/assets/00520049-0941-4a48-9b72-b548edcb0949)

2. Copy the generated link, paste it in the search box on the Home page and click Add. Wait for the program to go through all pages of the search link and collect all vehicles.

![guide-img-2](https://github.com/user-attachments/assets/6ddedd2f-e259-4fc5-91a9-fb4cc5dea8ed)

3. If everything went well, you should see the first 40 vehicles on the front page for each search link.

![guide-img-3](https://github.com/user-attachments/assets/cf9681db-b9c5-4c16-92af-d6a7e7fd1304)

4. When you open the vehicle page, you can see all the relevant information, including price history.

![guide-img-4](https://github.com/user-attachments/assets/7b51524f-5d77-46a9-ad7b-f529ab047d92)

5. There is also a Favorites page where you can add or remove vehicles by pressing the heart icon.

![guide-img-5](https://github.com/user-attachments/assets/0756c759-6702-49d0-943c-886c4e1e4dc5)
   
I've also implemented a background service to update the database for each user search link automatically. Every 15 minutes, it does a small search and stops after finding a matching vehicle ad, and every hour it performs a full search to update the database with new ads, changed prices and images. 

Users will receive a message to refresh the page when new ads are found. They can also decide if they want to receive emails with the relevant new ads (I'm using SendGrid for that).

Users can manually refresh a search link. Each refresh has a 15-minute cooldown.

The rest of the features you can check for yourself.

This repository is licensed under the Creative Commons Attribution-NonCommercial 4.0 International Public License. See the [LICENSE](./LICENSE) file for details.
