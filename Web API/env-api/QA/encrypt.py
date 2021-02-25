class Hash:
    def encrypt(string):
        asc = [ord(ch) for ch in string] # Converts each individual character to it's ASCII equivalent
        encrypted = [int(ASC) * 2 + len(string) for ASC in asc] # Doubles the ASCII equivalent of each character then adds the length of the parent string to it.
         # Joins all the encrypted values with '+' symbols

        string = ""
        for num in encrypted:
            if num == encrypted[-1]:
                string += str(num)
            else:
                string += f"{str(num)}+" 
                
        return string

    def decrypt(string):
        encrypted = string.split("+")
        # Spilts the encrypted values by the '+' symbols.
        reversed = [int((int(ch) - len(encrypted))/2) for ch in encrypted] # Decrypts it.
        decrypted = [chr(asciiCharacter) for asciiCharacter in reversed] # Decrupts it to a string

        return "".join(decrypted) # returns plain text
