const baseUrl = "./api/users"

var IsChekBTN = false;//האם יש כפתור בדיקה או לא
var jsonOBJ;// יחזיק את הג'ייסון שקיבלנו מAI
var count = 0;//סופר איזה שאלה אנחנו
var questions; //משתנה של רשימת השאלות


async function GetJson() {
    //נשלוף ונשמור את הערך מתיבת הטקסט במשתנה
    const subject = document.querySelector('input[name="subject"]:checked').value;
    const charecter = document.querySelector('input[name="character"]:checked').value;
    const language = document.querySelector('input[name="language"]:checked').value;
    const prompt = {
        "Subject": subject,
        "charecter": charecter,
        "Language": language,
    }

    //נשמור את הנתיב לAPI
    const url = baseUrl + "GPT/GPTChat"
    //נשמור את הפרמטרים לשליחה
    //שיטה מסוג POST
    //סוג תוכן: application/json
    //body: הנושא (בפורמט JSON)
    const params = {
        method: 'POST',
        headers: {
            Accept: 'application/json',
            'Content-Type': 'application/json'
        },

        // המרת הפרומפט לג׳ייסון
        body: JSON.stringify(prompt)
    }
    //נבצע את קריאת הfetch ונציג את השאלה בHTML
    //שמירה במשתנה מה שחוזר מהשרת
    const response = await fetch(url, params);

    if (response.ok) {
        console.log(response + "     קיבלנו OK");
        //המרה לפורמט מתאים
        let data = await response.json();
        console.log(data);
        jsonOBJ = JSON.parse(data); //הכנסונו את הדאטה למשתנה
        console.log(jsonOBJ);

       questions = jsonOBJ.Questions;// לוקח את רשימת השאלות

        console.log(questions[0]);

        //// Parse the JSON string into an object
        //const jsonResponse = JSON.parse(data);//זה הופך אותו לג'ייסון
        //console.log(jsonResponse);

    }
    else {
        console.log(await response.text());
    }
    getQuestion();
    document.getElementById("generate").disabled = true;
}



function getQuestion() {

    const chatResponsesContainer = document.getElementById('questions');//לוקח את הדיב מהHTML

    const questionElement = document.createElement('div');
    questionElement.innerHTML = `
     <p><strong>Question:</strong> ${questions[count].Qtext}</p>
     <button class="btn btn-outline-dark mr-4" onclick="handleAnswerClick('${questions[count].correct_ans}')">${questions[count].ans1}</button>
     <button class="btn btn-outline-dark mr-4" onclick="handleAnswerClick('${questions[count].correct_ans}')">${questions[count].ans2}</button>
     <button class="btn btn-outline-dark mr-4" onclick="handleAnswerClick('${questions[count].correct_ans}')">${questions[count].ans3}</button>
     <button class="btn btn-outline-dark" onclick="handleAnswerClick('${questions[count].correct_ans}')">${questions[count].ans4}</button>
     <hr>`;
    
    console.log(count);
    console.log(questionElement.innerHTML);
    chatResponsesContainer.appendChild(questionElement);
}




function handleAnswerClick(correctAnswer) {
    const clickedButton = event.target;
    const allButtons = document.querySelectorAll('button');
    const quizContainer = document.getElementById("questions");
   

    // Enable all buttons again
    allButtons.forEach((button) => {
        button.disabled = false;
    });
    document.getElementById("generate").disabled = true;

    const checkButton = document.getElementById("chekBTN");
    if (!checkButton) {
        // Show the "Check" button only if it doesn't exist
        const checkButton = document.createElement('button');
        checkButton.textContent = 'Check';
        checkButton.classList.add('btn', 'btn-outline-dark'); // Add the class
        IsChekBTN = true;
        checkButton.id = "chekBTN";
        quizContainer.appendChild(checkButton);
        checkButton.onclick = () => {
        if (clickedButton.textContent === correctAnswer) {
     
            //יצירת דיב להכלת המשוב (משתנה)
            const responElemnt = document.createElement('div');

            //ניקח את המשוב מהג'ייסון
            //נכניס את המשוב מהג'ייסון לדיב

            responElemnt.innerHTML = `<p><strong>Correct Answer:</strong> ${questions[count].correctRes}</p>`;
            console.log(responElemnt);
            console.log(responElemnt.innerHTML);

            const chatResponsesContainer = document.getElementById('questions');//לוקח את הדיב מהHTML

            //נכניס את הדיב לHTML
            chatResponsesContainer.appendChild(responElemnt);
            disablebtn();

            removBTN();

            //ניצור כפתור
            const nextButton = document.createElement('button');
            nextButton.textContent = 'next';
            nextButton.id = "nxtBTN";
            nextButton.classList.add('btn', 'btn-outline-dark'); // Add the class
            quizContainer.appendChild(nextButton);
            nextButton.addEventListener('click', nextBtn);


        } else {       

            //יצירת דיב להכלת המשוב (משתנה)
            const responElemnt = document.createElement('div');

            //ניקח את המשוב מהג'ייסון
            //נכניס את המשוב מהג'ייסון לדיב

            responElemnt.innerHTML = `<p><strong>InCorrect Answer:</strong> ${questions[count].wrongResp}</p>`;
            console.log(responElemnt);
            console.log(responElemnt.innerHTML);

            const chatResponsesContainer = document.getElementById('questions');//לוקח את הדיב מהHTML

            //נכניס את הדיב לHTML
            chatResponsesContainer.appendChild(responElemnt);
            disablebtn();
            removBTN();

            //ניצור כפתור
            const nextButton = document.createElement('button');
            nextButton.textContent = 'next';
            nextButton.id = "nxtBTN";
            nextButton.classList.add('btn', 'btn-outline-dark')
            quizContainer.appendChild(nextButton);
            nextButton.addEventListener('click', nextBtn);
            }
            count++;
        };
    }
}




function disablebtn() {
    // Your existing code for handling the answer click

    // Disable all buttons
    const buttons = document.querySelectorAll('button');
    buttons.forEach(button => {
        button.disabled = true;
    });
} // הופך כפתורים ללא פעילים



function removBTN() {
    const BTN = document.getElementById("chekBTN");
    BTN.remove();
    IsChekBTN = false;
} //מעלים את הכפתור





function nextBtn() {

    
   //  להעלים את השאלה הקודמת והתשובה שלה
    const questionsDiv = document.getElementById('questions');
    questionsDiv.innerHTML = ''; // Removes all child elements



    if (count >= 4) {
        //    alert("you finsh!");
        // Call the function to create and show the modal
        createModal();

    } else {
  //   להוציא את השאלה הבאה
    getQuestion();
    }
}


// Create a function to generate the modal
function createModal() {
    // Create the modal structure
    const modal = document.createElement("div");
    modal.classList.add("modal", "fade");
    modal.tabIndex = -1;
    modal.role = "dialog";
    modal.innerHTML = `
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Completed!</h5>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">×</span>
                    </button>
                </div>
                <div class="modal-body">
                    <p>You have finished answering all the questions!</p>
                    <p>Congratulations!</p>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-dark" id="redirectToPage">Go to Another Page</button>
                </div>
            </div>
        </div>
    `;

    // Append the modal to the body
    document.body.appendChild(modal);

    // Show the modal
    $(modal).modal("show");

    // Add event listener to the button for redirection
    const redirectToPageButton = modal.querySelector('#redirectToPage');
    redirectToPageButton.addEventListener('click', redirectToPage);
}

function redirectToPage() {
    // Change the URL to the path of your bored.html file
    window.location.href = "bored.html";
}





async function GetActivity() {

    const baseUrl = "./api/users"

    const url = baseUrl + "gpt/bored";
    const param = {
        method: 'GET',
        headers: {
            Accept: 'application/json',
        },
    }

    const response = await fetch(url, param);
    if (response.ok) {
        const data = await response.json();
        const activity = data["activity"];
        const type = data["type"];
        const participants = data["participants"];

        document.getElementById("act").innerHTML = activity;
        document.getElementById("type").innerHTML = type;
        document.getElementById("partici").innerHTML = participants;

    }
    else {
        console.log(errors);
    }


}