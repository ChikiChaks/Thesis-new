using System;
namespace Prog3_WebApi_Javascript.DTOs
{
    //ניצור DTO נוסף עבור הprompt שמגיע מהמשתמש
    //כרגע הוא יכיל רק מאפיין של תוכן, בהמשך נוכל להוסיף עוד מאפיינים

    public class Prompt
    {
        public string Subject { get; set; }
        public string Charecter { get; set; }
        public string Language { get; set; }
    }

}

