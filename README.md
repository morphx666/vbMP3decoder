# vbMP3Decoder

Original Release: Jan/27/2006  
Latest Update: July/15/2010

![](https://whenimbored.xfx.net/wp-content/uploads/2016/07/vbMP3Decoder-ss.png)
    
**What is vbMP3Decoder?**

Well, as its name suggests it, it's an MP3 decoder written in Visual Basic.  
The original Visual Basic 6 code is a port from 
[Krister Lagerström](http://www.kmlager.com/)'s C version 
of the decoder and it was made by Murphy McCauley from
http://www.constantthought.com/

Murphy's original VB6 code worked perfectly and was able to convert an MP3 
file into a WAV file. I then modified the code so that instead of writing the PCM 
data to a file, it would write it to a DirectSound buffer. Unfortunately, VB6's 
code was extremely slow -- the CPU resources raised to 100% and the decoder 
wasn't able to play a single MP3 file without skipping.  
So, I decided to port the code to VisualBasic.NET 2005 and see if I could 
take advantage of the new array handling routines, among other stuff. After 
playing a bit with the code I have been able to not only make it work, but to 
reduce the CPU consumption to less than 15%.  
Later on I rewrote a lot of the code to take advantage of the new features in 
recent versions of the .NET, making it simpler, cleaner and even faster.

**What can I use this for?**

To learn...  
The speed and quality of the decoder are not optimal for production use so I'd 
suggest you use this code to learn and of course, to admire how VB, and the whole .NET framework, have evolved!
