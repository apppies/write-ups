import java.math.BigDecimal;
import java.math.RoundingMode;
import java.security.MessageDigest;
import java.security.Key;

import javax.crypto.Cipher;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;
import java.util.Base64;

val StringToDecode = "Q/aaG1P19kzJV7Szeg63oIUZs9gSo+0T3kYZ2Q8Qlj1NVUYzvoCJSUWI0bPRdr4p";

fun main() {
 	val sequence = generateSequence(1, { it + 1 }).map{getRange(it)};
    val iterator = sequence.iterator()
    
    var next = iterator.next();
    var old = next;
    while (iterator.hasNext()) {
        next = next +  iterator.next();

        //Fix:dont continue when difference gets small
        if ( (next - old).compareTo(BigDecimal("0.00001")) < 0)
        {
            break;
        }
        old = next;      
    }

    var bigIntegerString = next.setScale(0, RoundingMode.HALF_UP).toBigInteger().toString();

	var key = SHA256Hash(bigIntegerString);
	var bytesToDecode = Base64.getDecoder().decode(StringToDecode);
	var decodedBytes = AESCipher(2, bytesToDecode, key);
	
	println(String(decodedBytes, Charsets.UTF_8));
}

fun getRange(n: Int): BigDecimal {
    var a = BigDecimal("58498716739671875697238968542");
    var d = BigDecimal("7868849012967");
    var b = BigDecimal("91957681313867825728");
    var r = BigDecimal("0.5");
    return b * (a + d * BigDecimal(n - 1)) * r.pow(n - 1);
}

fun SHA256Hash(input: String): ByteArray {
    var bytes = input.toByteArray()
    var md = MessageDigest.getInstance("SHA-256")
    var digest = md.digest(bytes)
    return digest
}

fun AESCipher(mode: Int, plainText: ByteArray, key: ByteArray): ByteArray {
    var c = Cipher.getInstance("AES/CBC/PKCS5Padding");
    c.init(mode, SecretKeySpec(key, "AES"), IvParameterSpec(byteArrayOf(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)));
    return c.doFinal(plainText);
}