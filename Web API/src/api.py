"""
Check requirements.txt for library details.
"""
# TODO: error handing during execute of sql queries, whether it be sqLITE or MySQL

from flask import Flask, session
from flask_restful import Api 

import os
import socket

import QA

# from endpoints.tap import Tap
# from endpoints.tinvoicehistory import TInvoiceHistory
# from endpoints.sync import Sync
# from endpoints.spec import Spec
from endpoints.insertGeneric import InsertGeneric
from endpoints.login import Login
from endpoints.upload import Upload
from endpoints.mock import Mock

app = Flask(__name__) # Initialisation of a flask app
api = Api(app) # Initilisation of a RESTful API

# Adding resources
"""
This is for any request, but, is most significant for a GET request. In the second parameter you can see the following:
/<string:debtorCode>
Which means that after the domain name of the website. Add a slash followed by a valid debtor code. And the result, will be returned.
In the form of JSON.
"""

# Commented out means fixed
# Add key, and user params to all url's except spec, and login for key validation.
# api.add_resource(Tap, "/tap/<string:user>/<string:hash>")
# api.add_resource(TInvoiceHistory, "/get/tinvoicehistory/<string:invoiceNumber>")
# api.add_resource(Sync, "/sync/<string:salesperson>")
# api.add_resource(Spec, "/get/spec/<string:table>/<string:o1>/<string:o2>/<string:o3>/<string:o4>/<string:o5>")
api.add_resource(InsertGeneric, "/redirect/<string:table>/<string:invoiceno>")
api.add_resource(Login, "/<string:user>/<string:password>")
api.add_resource(Upload, "/upload/<string:saveName>")
api.add_resource(Mock , "/mock/<string:third>")

@app.route("/") # Default page
def hello():
    return "API of TSC"

@app.route("/password")
def password():
    return "Password here: 123"

if __name__ == "__main__":
    currentHost = socket.gethostbyname(socket.gethostname())
    app.run(debug = True, host=currentHost)
