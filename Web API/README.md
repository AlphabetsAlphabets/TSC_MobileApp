# Syncing SQLite and MySQL databases.
The purpose of this syncing is to be able to update the global catalogue of transactions done by each employee to the central database hosted in a server. To keep track of transactions that are completed, and to verify it's legitamacy, reduce costs. As it is an inherent side effect of syncing two databases.

# How it works.
The sync is monodirectional (pseudo-bi-directional). Meaning only one table gets updated. 
When a salesperson completes a sale, they will add it to their local SQLite table via a mobile app.

Once they decide to finish their day, and press the sync button on the app, and all the new entries will be added (synced) to the MySQL database hosted on a server.

# Extending the API
If you want to develop additional endpoints. Refer to developementGuide.md. There will be explicit instructions as to the naming conventions
of variables. As well as, the __complete explanation of one__ endpoint. For you to get an idea of how it works. This also includes explanation of
what the modules do, third party or not.