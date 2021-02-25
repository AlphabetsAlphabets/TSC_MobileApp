# Schema: tsc_office
from flask_restful import Resource  # pip install flask-restful

from QA.database import Database
from QA.key import Key

class Login(Resource):
    def __init__(self):
        """Connects to the MySQL database"""
        # self.tap, self.tapCursor = Database.connect("localhost", "root", "8811967", "tsc_office")
        self.tap, self.tapCursor = Database.connect("localhost", "root", "YJH030412yjh_g", "tsc_office")

    def get(self, user, password):
        enc, k = Key().getKey(user, password)

        sqlString = f"SELECT * FROM tsc_office.tap WHERE fuser = '{user}' AND fpassword = '{enc}'"
        self.tapCursor.execute(sqlString)

        fid = self.tapCursor.fetchall()[0][4]

        return {"fid": fid, "key": k}
