/*
Problem 1: Multiples of 3 or 5
If we list all the natural numbers below 10 that are multiples of 3 or 5, we get 3, 5, 6 and 9. The sum of these multiples is 23.

Find the sum of all the multiples of 3 or 5 below the provided parameter value number.

Tests:
Waiting:1. multiplesOf3Or5(10) should return a number.
Waiting:2. multiplesOf3Or5(49) should return 543.
Waiting:3. multiplesOf3Or5(1000) should return 233168.
Waiting:4. multiplesOf3Or5(8456) should return 16687353.
Waiting:5. multiplesOf3Or5(19564) should return 89301183.
*/

console.log("multiplesOf3Or5(10): "+ multiplesOf3Or5(10));
console.log("multiplesOf3Or5(49): "+ multiplesOf3Or5(49));
console.log("multiplesOf3Or5(1000): "+ multiplesOf3Or5(1000));
console.log("multiplesOf3Or5(8456): "+ multiplesOf3Or5(8456));
console.log("multiplesOf3Or5(19564): "+ multiplesOf3Or5(19564));

function multiplesOf3Or5(number){
    let sum = 0;
    console.log(`Calculating multiples of 3 or 5 below ${number}...`)
    for(let i = 0; i < number; i++){
        if(i % 3 == 0 || i % 5 == 0){
            sum += i;
            console.log(`Adding ${i}, current sum: ${sum}`);
        }
    }
    console.log(`Final sum of multiples of 3 or 5 below ${number}: ${sum}`);
    return sum;
}

