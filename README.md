# This is a learning project I created to learn how background service work

It is fairly easy to know how it work, but I did _not_ trust the knowledge if I have not touch the code base.

You will also notice that this markdown file is also a learning project. Very weird but a 4 year exp developer does not do mark down.

I go with the flow, and right now I think it is a good time to learn the basic.

## About the project

1. ### Change tracking

Pooling from database and keep a timestamp on current database to keep track. Which is a background service. A **singleton** service that run until the application die, or we exit it via cancellation token.

2. ### Publisher

Publish the changes to kafka topic. With is a service injected into the change tracking to publish the changes via message.

3. ### Consumer

Read from the kafka topic and print out in console. Which is a background service. A **singleton** service that run until the application die, or we exit it via cancellation token.

Every consumer must also be a job that run always.

4. ### Kafka

A broker saving message. I also just learn about kafka so I think I want to give it a try. The next project will be learn create a scalable project using kafka

5. ### Container

Yea, docker is also a new thing I am trying. Bundle everything and build **images**, which docker will use as a guide to build **containers**. These containers are the small virtual machine that run the code.
