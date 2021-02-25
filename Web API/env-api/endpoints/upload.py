from logging import raiseExceptions
from flask_restful import Resource, abort, reqparse
from werkzeug.datastructures import FileStorage

from PIL import Image

from QA.database import Database

import os
import tempfile as file

import datetime

# Types
from PIL.JpegImagePlugin import JpegImageFile as TYPE_JPEG_IMAGE
from PIL.Image import Image as TYPE_IMAGE

"""
Either an additional column (or dedicated) table is needed to store the path to the images.
VARCHAR(250) recommended.
"""

class Upload(Resource):
    def __init__(self):
        self.schema = "testing"
        self.table = "images"

        self.conn, self.cursor = Database.connect("localhost", "root", "YJH030412yjh_g", self.schema)

        parser = reqparse.RequestParser()
        parser.add_argument("image", type=FileStorage, location="files")

        self.parsed = parser.parse_args()

        try:
            path = "../src/images/"
            os.chdir(path)
            print("CHANGED PATH")
        except Exception:
            pass

    def __info(self, image):
        print(f"content_length: {image.content_length}")
        print(f"content_type: {image.content_type}")
        print(f"filename: {image.filename}")
        print(f"headers: {image.headers}")
        print(f"mimetype: {image.mimetype}")
        print(f"mimetype_params: {image.mimetype_params}")
        print(f"name: {image.name}")
        print(f"stream: {image.stream}")


    def __resize(self, dim: tuple) -> tuple:
        width, height = dim

        value = max(width, height)
        if (value > 2000):
            ratio = 2000 / value
            newWidth = width * ratio
            newHeight = height * ratio

            return (int(newWidth), int(newHeight))

        return dim

    def __startup(self, saveName):
        print("\n")
        banner = "=="*20 + " NEW PUT REQUEST " + "=="*20

        print(banner)
        print("- API: Upload")
        print(f"Current date and time: {datetime.datetime.now()}")
        print("\n" + f"saveName: {saveName}")
        print("=" * len(banner))

    def put(self, saveName):
        self.__startup(saveName)

        saveName = saveName.strip(".jpg").strip(".jpeg")

        image = self.parsed["image"]
        if image is None:
            abort(406, message="You have not uploaded an image.", code=406)
        # self.__info(image)

        imageSave = os.getcwd() + f"\\{saveName}.jpg"

        rawImage = Image.open(image.stream)
        dim = (int(rawImage.width), int(rawImage.height))

        newDim = self.__resize(dim)

        resized = rawImage.resize(newDim, Image.ANTIALIAS)
        resized.save(imageSave)

        # add path to my sql
        pathToImage = (os.getcwd() + f"\\{saveName}.jpg").split("\\")
        sqlCompliantPath = ("/").join(pathToImage)

        sqlQuery = f"INSERT INTO {self.schema}.{self.table} (`image name`, `image path`) VALUES ('{image.name}', '{sqlCompliantPath}')"
        self.cursor.execute(sqlQuery)
        self.conn.commit()

        return {"success": "image succesfully uploaded", "code": 200, "path": sqlCompliantPath}

