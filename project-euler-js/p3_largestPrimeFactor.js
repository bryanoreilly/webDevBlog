/*
    Problem 3: Largest prime factor
    The prime factors of 13195 are 5, 7, 13 and 29.

    What is the largest prime factor of the given number?

    Tests:
    Waiting:1. largestPrimeFactor(2) should return a number.
    Waiting:2. largestPrimeFactor(2) should return 2.
    Waiting:3. largestPrimeFactor(3) should return 3.
    Waiting:4. largestPrimeFactor(5) should return 5.
    Waiting:5. largestPrimeFactor(7) should return 7.
    Waiting:6. largestPrimeFactor(8) should return 2.
    Waiting:7. largestPrimeFactor(13195) should return 29.
    Waiting:8. largestPrimeFactor(600851475143) should return 6857.
*/

console.log(largestPrimeFactor(2))
console.log(largestPrimeFactor(3))
console.log(largestPrimeFactor(7))
console.log(largestPrimeFactor(13195))
// console.log(largestPrimeFactor(35))
// console.log(largestPrimeFactor(600851475143))

function largestPrimeFactor(number){
    factors = []
    console.log(`calcluating prime numbers of ${number} ...`)
    if (number>1) factors.push(2)
    let largestPrimeFactor = 0;

    for(let i = 2; i <= number; i++){
        if(number%i == 0){
            // if(&& i%2 != 0 && i%3 !=0 && i%5 !=0 && i%7 !=0) {}
            factors.push(i);
            while(number%i == 0){
                console.log("number to be floored number: " + number)
                // number = Math.floor(number/i);
                number = number / i;
            }
            console.log("current factor of number " + i);
        }
    }
    console.log("prime factors: "+ factors)
    largestPrimeFactor = factors.pop();
    console.log(`final largest prime factor of ${number} is: \n${largestPrimeFactor}`)
    // return largestPrimeFactor;
    return factors;
}