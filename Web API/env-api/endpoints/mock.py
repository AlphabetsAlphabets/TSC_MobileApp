import datetime

from PIL import Image

from flask_restful import Resource, reqparse, abort
from werkzeug.datastructures import FileStorage

class Mock(Resource):
    def __init__(self):
        parser = reqparse.RequestParser()
        parser.add_argument("first")
        parser.add_argument("second")
        parser.add_argument("fourth", type=FileStorage, location="files")

        self.parsed = parser.parse_args()

    def put(self, third):
        print("=="*20 + " NEW GET REQUEST " + "=="*20)
        print("- API: MOCK")
        print(f"- Current date and time: {datetime.datetime.now()}")

        first, second, fourth = self.parsed["first"], self.parsed["second"], self.parsed["fourth"]
        print(f"\nfirst: {first}, second: {second}, third: {third}")
        print(f"Fourth: {fourth}")
        print("=="*48)

        path = f"../src/images/{third}.jpg"
       # C:\Users\YAP JIA HONG\Documents\coding\python\Web API\src\images 

        if fourth is None:
            abort(406, message="bad")
        image = Image.open(fourth.stream)
        image.save(path)

        return {"message": "request successful", "code": 200}