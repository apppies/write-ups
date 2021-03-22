# L&#39;aritmetico, Il geometrico, Il finito

## Problem

https://thenixuchallenge.com/c/aritmetico/

While everyone was on summer holiday, a particularly clever intern came up with a tool to protect our deployment backup keys. The solution has of course reached production already, prime showcase of how agile we are! There is only one worrying thing: the test run in CI still hasn't finished and it's already winter... Better take a look!

## Solution

The main iteration does not finish but also does not change the answer after a certain amount of iterations. Add an exit condition to the loop and let the code give the answer.

### Introduction

We are provided with a jar file. So first step is to decompile it and obtain the source code. For that the site http://www.javadecompilers.com is very useful. Upload the OHO.jar, select a decompiler and download the source code. It takes several tries to find the decompiler which gives the best result. It appears to be Jadx.

Once decompiled we find three folders: kotlin, org (containing JetBrains) and p000. So the program has been programmed in the language Kotlin using a tool from JetBrains.

Let's analyze the code we got from the decompiler. It consists of three files:

  - SenzaLimitiKt$main$x$1.java
  - SenzaLimitiKt$main$x$2.java
  - SenzaLimitiKt.java

The first two appear to be some sort of helper classes, the last one contains the main entry.
Both helper classes are only called on line 97 in SenzaLimitiKt.java:

```java
Iterator iterator$iv = SequencesKt.map(SequencesKt.generateSequence((Object) UNO, (Function1) SenzaLimitiKt$main$x$1.INSTANCE), SenzaLimitiKt$main$x$2.INSTANCE).iterator();
```

$1 is used to generate a sequence, $2 is mapped to that sequence. 

### SenzaLimitiKt$main$x$1.java

Cleaning up the code a bit results in the following class

```java
final class SenzaLimitiKt$main$x$1 extends Lambda implements Function1<BigDecimal, BigDecimal> {
    public static final SenzaLimitiKt$main$x$1 INSTANCE = new SenzaLimitiKt$main$x$1();

    SenzaLimitiKt$main$x$1() {
        super(1);
    }
    
    public final BigDecimal invoke(BigDecimal it) {
        BigDecimal add = it.add(SenzaLimitiKt.getUNO());
        return add;
    }
}
```

The `Instance` being called in the main code runs the constructor. The `generateSequence` calls the `invoke` function and provides it with a BigDecimal. The `invoke` function calls the function `add` with parameter `SenzaLimitiKt.getUno()`. Looking up that getter shows us that it returns a `1`.
This lambda class adds one to the value it is given. Passing this to the generateSequence results in an infinite list of increasing numbers.

### SenzaLimitiKt$main$x$2.java

Cleaning up the code a bit results in the following class

```java
final class SenzaLimitiKt$main$x$2 extends Lambda implements Function1<BigDecimal, BigDecimal> {
    public static final SenzaLimitiKt$main$x$2 INSTANCE = new SenzaLimitiKt$main$x$2();

    SenzaLimitiKt$main$x$2() {
        super(1);
    }

    public final BigDecimal invoke(BigDecimal it) {
        return SenzaLimitiKt.SGludDogZG9uJ3QgaG9sZCB5b3VyIGJyZWF0aCwgaXQgd2lsbCBuZXZlciBmaW5pc2guLi4K(it);
    }
}
```

Again a lambda function accepting a BigDecimal, only to pass it immediatly to a function with an incredible name in the main file to process it, return its return value. Might that name be a hint? Lets see if it is Base64 encoded. Ah yes, it decodes to `Hint: don't hold your breath, it will never finish...`. Good to know that we have to fix this function. Let's rename it to `getMapping`.

Using this knowledge, we can get rid of the two classes by rewriting line 97 in SenzaLimitiKt.java to:

```java
val sequence = generateSequence(1, { it + 1 }).map{getMapping(it)};
```

### SenzaLimitiKt.java

Let's analyze this file part by part. First the global variables.

```java
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
```

This defines two properties, one and zero. The set functions are never used and can be ignored. The get functions and variables are used at multiple places to provide a BigDecial zero and one. The string `chiaveDiImplementazione`, Italian for Implementation Key, is used in `main`. It turns out to be the secret that needs to be decoded.

Let's rewrite this to

```kotlin
val StringToDecode = "Q/aaG1P19kzJV7Szeg63oIUZs9gSo+0T3kYZ2Q8Qlj1NVUYzvoCJSUWI0bPRdr4p";
```

Next is the function with the long name.

```java
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
```

The name base64 decodes to `Hint: don't hold your breath, it will never finish...`. First, simplify it a bit to be more readable.

```kotlin
fun getMapping(n: Int): BigDecimal {
    var d = BigDecimal("7868849012967");
    var b = BigDecimal("91957681313867825728");
    var r = BigDecimal("0.5");
    var s = BigDecimal("1");
    var o = BigDecimal("58498716739671875697238968542");
    while (n.compareTo(1) > 0) {
        s = s * r;
        n = n - 1;
        o = o + d;
    }
    return o * b * s;
}
```

The function does some arithemics with large numbers and returns the answer. No problem there. This can obviously be optimized by expanding the while loop to a single formula.

```kotlin
fun getMapping(n: Int): BigDecimal {
    var d = BigDecimal("7868849012967");
    var b = BigDecimal("91957681313867825728");
    var r = BigDecimal("0.5");
    var o = BigDecimal("58498716739671875697238968542");
    return b * (o + d * BigDecimal(n - 1)) * r.pow(n - 1)
}
```

This function is called from SenzaLimitiKt$main$x$2 with the values from the generated sequence. The factor of `0.5` to the power of `n - 1` makes sure that the return value will go to zero for large n.

Next up is `cifrareperfavore` or `Please enter`. 

```java
public static final byte[] cifrareperfavore(int moda,  byte[] plaintext,  byte[] key) {
    Cipher c = Cipher.getInstance("AES/CBC/PKCS5Padding");
    c.init(moda, new SecretKeySpec(key, "AES"), new IvParameterSpec(new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}));
    byte[] doFinal = c.doFinal(plaintext);
    
    return doFinal;
}
```

The purpose it rather clear: it processes a byte array with the given key. Let's rename it to `AESCipher`.

Next one is `immaginareunachiave` or `Imagine a key`

```java
public static final byte[] immaginareunachiave( String p) {
    MessageDigest instance = MessageDigest.getInstance("SHA-256");
    byte[] bytes = p.getBytes(Charsets.UTF_8);
    
    return instance.digest(bytes);
}
```

It takes a string and return the SHA-256 hash. Therefore it is renamed to `SHA256Hash`

That brings us to `main`. Up to now no infinite loops or strange functions have been found. So the problem most likely resides in `main`.

```java
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
```

The function does a few things:

  1. Create an iterator over a generated mapping
  2. Sum all values from the mapping
  3. Get the integer part of the sum and make it a string
  4. Create a key from the string
  5. Create a byte array to decode
  6. Print the decoded value.

The first step is rewriting this into code that is better readable.

```kotlin
fun main() {
    val sequence = generateSequence(1, { it + 1 }).map{getMapping(it)};
    val iterator = sequence.iterator()

    var next = iterator.next();
    while (iterator.hasNext()) {
        next = next + iterator.next();
    }
    
    var bigIntegerString = next.setScale(0, RoundingMode.HALF_UP).toBigInteger().toString();
    
    var key = SHA256Hash(bigIntegerString);
    var bytesToDecode = Base64.getDecoder().decode(StringToDecode);
    var decodedBytes = AESCipher(2, bytesToDecode, key);
    
    println(String(decodedBytes, Charsets.UTF_8));
}
```

The generated sequence has infinite length. Iterating over infinite numbers and summing the values of numbers might take long. As the value of the getMapping function will go to zero an infinite summing will not change the integer part of summed value anymore for large n. Let's extend the while loop with a check to see if the summed value is not significantly increasing anymore.

```kotlin
    var next = iterator.next();
    var old = next
    while (iterator.hasNext()) {
        next = next + iterator.next();
        
        //Fix: dont continue when difference gets small
        if ( (next - old).compareTo(BigDecimal("0.00001")) < 0)
        {
            break;
        }
        old = next;
    }
```

The resulting kotlin source is given in [source.kt](source.kt). As I like C#, the C# code to obtain the same is given in [source.cs](source.cs).

Running the program will result in the flag.

```console
NIXU{#####_###_##_#####_########_#####_###}
```

Note: running it in kotlin might trigger something with an illegal key size. Use a newer version of the JDK and change the security settings to allow unlimited strength for cryptography
