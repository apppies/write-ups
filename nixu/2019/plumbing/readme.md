# Plumbing

## Problem
ur whole cloud infrastructure was accidentally deleted by an erroneous deployment script. We have tape backups but they are encrypted using a very secure passphrase and it turns out it was stored in a git repository in the cloud. We were able to construct a complete git log of the repository since a developer had setup an unauthorized bot that was quietly tweeting all commits to the world. The log is attached as gitlog.txt.
Can you help us recover the key?

[gitlog.txt](https://thenixuchallenge.com/c/plumbing/static/challenge/gitlog.txt)

## Solution
Recreate the commits by following the comments. Verify each commit by checking the hash. Either done by running git every time (slow) or by only doing the hash calculations (fast).

### Introduction
The gitlog gives the following information for every commit:
- Author
- Date
- Comment
- Hash

When looking at the anatomy of a git commit, it shows that the hash is given by the content of the repository and its history. So in theory it is doable to recreate the content of a repository and it's history by finding the data that produces the commit hash. [Burgdorf on ThoughTram.io](https://blog.thoughtram.io/git/2014/11/18/the-anatomy-of-a-git-commit.html), [Masak](https://gist.github.com/masak/2415865) and [domgetter](https://gist.github.com/masak/2415865#gistcomment-1448934) provide data on how the GIT commit hashes are formed, and that is what will be used to solve this problem. Luckily, our gitlog provides us with nice comments that describe what is to be expected from each commit, therefore reducing the guessing work greatly.

### Starting the repository

Every commit could be replayed by using GIT itself:

1. Create a repository
2. Add or change a file
3. Commit it with the right comment and author
4. Verify the hash
5. Repeat steps 2 to 4 until the hash is equal to the one in the gitlog.

This takes however multiple second per commit, and is therefore not fast enough once a dice with 10000000 sides has to be thrown.

To speed things up, a GIT simulation is needed that calculates the commit hash. A commit consists of the following items in the right formatting:

- tree hash representing the current files in the repository
- parent hash if available
- author
- date
- committer
- commit date
- comment

Note: for a commit only the parent hash is needed, not the complete history of each file! Therefore, if the parent hash is known, the files of the repository at a given state can be recreated without knowing the previous versions! This is what is used below to generate commit hashes and to recreate the git repository.

#### Tree Hash

The tree hash represents the current files in a commit. Using the answer on (https://stackoverflow.com/questions/14790681/what-is-the-internal-format-of-a-git-tree-object) the following C# functions are derived to create a tree hash. We only have a simple repository with one file, so no complete tree hash functionality is needed.

The format for a tree hash is given by (new lines added for readability!):

```
tree length_of_tree_in_bytes\0
file_mode file_name\0sha1_hash
file_mode file_name\0sha1_hash
file_mode file_name\0sha1_hash
...
```

With the sha1 hash created by calculating the SHA1 hash of `blob file_length\0file_content`

In C# code this becomes:

```cs
static byte[] Sha1FromByteArray(byte[] input)
{
	var hash = new SHA1Managed().ComputeHash(input);
	return hash;
}

static byte[] GetGITFileHash(string content)
{
	var input = $"blob {content.Length}\0" + content;
	var byteyinput = Encoding.UTF8.GetBytes(input);
	return Sha1FromByteArray(byteyinput);
}

static byte[] TreeHash(Dictionary<string,string> files)
{
	//Construct bytes tree
	var tree = new List<byte>();
	foreach (var file in files)
	{
		var fileEntry = $"100644 {file.Key}\0";
		var fileHash = GetGITFileHash(file.Value);
		var leaf = Encoding.ASCII.GetBytes(fileEntry).ToList();
		leaf.AddRange(fileHash);
		tree.AddRange(leaf);
	}
	var header = Encoding.ASCII.GetBytes($"tree {tree.Count}\0").ToList();
	header.AddRange(tree);

	return header.ToArray();
}
```

#### Commit hash

To calculate the entire commit and the commit hash, the tree hash and additional information about the commit is needed and added to the commit.

1. Obtain the tree hash and represent its SHA1 hash hexadecimal
2. Construct the commit info using the following format:

```
tree tree_hash\nparent parent_hash\nauthor author_name date\ncommitter commit_author commitdate\n\ncomment_text\n
```

3. Create the entire commit string using

```
commit commitinfo_length\0commitinfo
```

4. Obtain the commit hash by calculating the SHA1 hash of the commit string.

In C# code:

```cs
static string Sha1StringFromString(string input)
{
	var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(input));
	return string.Concat(hash.Select(b => b.ToString("x2")));
}
		
static string CommitHash(CommitInfo commit, string FlagContent)
{
	var tree = TreeHash(new Dictionary<string, string>() { { "flag.txt", FlagContent } });

	var treeHash = string.Concat(Sha1FromByteArray(tree).Select(b => b.ToString("x2")));

	var commitInfo = "";
	if (commit.Parent != "")
	{
		commitInfo = $"tree {treeHash}\nparent {commit.Parent}\nauthor {commit.Author} {commit.Date}\ncommitter {commit.Author} {commit.Date}\n\n{commit.Comment}\n";
	}
	else
	{
		commitInfo = $"tree {treeHash}\nauthor {commit.Author} {commit.Date}\ncommitter {commit.Author} {commit.Date}\n\n{commit.Comment}\n";
	}

	var commitString = $"commit {commitInfo.Length}\0{commitInfo}";

	var commitHash = Sha1StringFromString(commitString);

	return commitHash;
}
```	

### Back to the problem

With the GIT code present a git commit hash can be calculated from a given collection of files with content, the author, date and comment. Time to follow the commits from the gitlog.

Source code for reading the gitlog.txt and solving the first part is available [here](plumbing.cs)

#### Commit 0

The first commit is adding an empty file `flag.txt`. No need to simulate.

#### Commit 1 to 25
Append a number between 1 and 9, 1 and 1000, or 1 and 10000000.

Assuming flag.txt is appended with a number. Repeating for every commit:

```
flag = current flag
targetcommithash = get hash from gitlog
sides = 9, 1000 or 10000000
for i = 1 to sides
	newflag = flag + i
	newcommithash = calculate hash commit
	if newcommithash = targetcommithash then
		update current flag with newflag value
		break
	end if
next
```

```
flag = 689676399780243364502385099687264621047025008848296702816833952558388938389548485877060426128011030487330314
```

#### Commit 26

"Encode the number as hex to save space"

Rebase the number to base 16. The problem is that a normal integer can't hold the number anymore, so the BigInteger class is used.

```cs
BigInteger bigInt = BigInteger.Parse(flag);
flag = bigInt.ToString("x");
```

```
flag = 4b2d656e6c0a787679620a50756e657976720a717279676e0a7a62776e0a476e61620a6776666e0a666e6f6e0a
```

#### Commit 27

"Encode as raw bytes to save even more space"

Take two characters from the hex string, interpret them as byte, repeat for all character and append, interpret the result as string.

```cs
var toBytes = new List<byte>();
for (int i = 0; i < flag.Length; i+=2)
{
	var sub = $"{flag[i]}{flag[i + 1]}";
	var subnum = Convert.ToByte(sub, 16);
	toBytes.Add(subnum);
}
string bitString = Encoding.ASCII.GetString(toBytes.ToArray());
```

```
flag = K-enl\nxvyb\nPuneyvr\nqrygn\nzbwn\nGnab\ngvfn\nfnon\n
```

#### Commit 28

"Encrypt with rot13"

Apply a ROT13 encryption, either code something or use an online tool

```
flag = X-ray\nkilo\nCharlie\ndelta\nmoja\nTano\ntisa\nsaba\n
```

Nice, finally something readable.

#### Commit 29

"Translate from Swahili to English"

Aha, the moja, Tano, tisa and saba are Swahili! Using google translate to get to

```
flag = "X-ray\nkilo\nCharlie\ndelta\none\nFive\nnine\nseven\n";
```

#### Commit 30

"Decode phonetic alphabet (lowercase, no whitespace)"

```
xkcd1597
```

See [XKCD 1597](https://xkcd.com/1597/) for a proper git manual.

#### Commit 31

"Wrap inside NIXU{}"

```
NIXU{########}
```