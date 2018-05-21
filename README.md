# payment_behpardakht
نحوه اتصال به بانک ملت در پروژ ه های Asp.net MVC

this project uses local sql server in visual studio, 
I generated a model which is database first containing just two table(basket and payment) as a sample for you,of course it would be different with your db.
first,add a webrefrence to behpardakhte mellat gateway.
then, in behpadakht class set the otherid and username and password that is given to you through email from the bank.
third,is your callbackurl which means the page of you site that user will be redirected after bank.
there is just two methods that you need to use : 1-connecting to bank(bpayRequest)
                                                 2-verifying payment(bpayVerify)
                                                 
                                                 

