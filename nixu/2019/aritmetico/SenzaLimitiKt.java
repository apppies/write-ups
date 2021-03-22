package p000;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.security.MessageDigest;
import java.util.Base64;
import java.util.Iterator;
import javax.crypto.Cipher;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;
import kotlin.Metadata;
import kotlin.jvm.functions.Function1;
import kotlin.sequences.SequencesKt;
import kotlin.text.Charsets;
import org.jetbrains.annotations.NotNull;

public final class SenzaLimitiKt {    
    private static BigDecimal UNO = new BigDecimal(1);
    private static BigDecimal ZERO = new BigDecimal(0);
    public static final String chiaveDiImplementazione = "Q/aaG1P19kzJV7Szeg63oIUZs9gSo+0T3kYZ2Q8Qlj1NVUYzvoCJSUWI0bPRdr4p";
	
    public static final BigDecimal getUNO() {
        return UNO;
    }

    public static final void setUNO( BigDecimal bigDecimal) {
        
        UNO = bigDecimal;
    }
    
    public static final BigDecimal getZERO() {
        return ZERO;
    }

    public static final void setZERO( BigDecimal bigDecimal) {   
        ZERO = bigDecimal;
    }

    public static final BigDecimal SGludDogZG9uJ3QgaG9sZCB5b3VyIGJyZWF0aCwgaXQgd2lsbCBuZXZlciBmaW5pc2guLi4K( BigDecimal n) {    
        BigDecimal a = new BigDecimal("58498716739671875697238968542");
        BigDecimal d = new BigDecimal("7868849012967");
        BigDecimal b = new BigDecimal("91957681313867825728");
        BigDecimal r = new BigDecimal("0.5");
        BigDecimal s = UNO;
        BigDecimal nn = n;
        BigDecimal o = a;
        while (nn.compareTo(UNO) > 0) {
            s = s.multiply(r);
            nn = nn.subtract(UNO);       
            o = o.add(d);
        }
        BigDecimal multiply = o.multiply(b);
        BigDecimal multiply2 = multiply.multiply(s);
        
        return multiply2;
    }
    
    public static final byte[] cifrareperfavore(int moda,  byte[] plaintext,  byte[] key) {
        Cipher c = Cipher.getInstance("AES/CBC/PKCS5Padding");
        c.init(moda, new SecretKeySpec(key, "AES"), new IvParameterSpec(new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}));
        byte[] doFinal = c.doFinal(plaintext);
        
        return doFinal;
    }

    public static final byte[] immaginareunachiave( String p) {
        MessageDigest instance = MessageDigest.getInstance("SHA-256");
        byte[] bytes = p.getBytes(Charsets.UTF_8);
        
        return instance.digest(bytes);
    }

    public static final void main( String[] args) {
        System.out.println("calcolo chiave segreta... aspettalo");
        Iterator iterator$iv = SequencesKt.map(SequencesKt.generateSequence((Object) UNO, (Function1) SenzaLimitiKt$main$x$1.INSTANCE), SenzaLimitiKt$main$x$2.INSTANCE).iterator();
		
        Object next = iterator$iv.next();
        while (iterator$iv.hasNext()) {
            next = ((BigDecimal) next).add((BigDecimal) iterator$iv.next());    
        }
		
        String bigInteger = ((BigDecimal) next).setScale(0, RoundingMode.HALF_UP).toBigInteger().toString();
        
        byte[] k = immaginareunachiave(bigInteger);
        byte[] decode = Base64.getDecoder().decode(chiaveDiImplementazione);
        
        System.out.println(new String(cifrareperfavore(2, decode, k), Charsets.UTF_8));
    }
}
