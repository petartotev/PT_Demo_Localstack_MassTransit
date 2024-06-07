#!/bin/bash
echo 'Starting SQS...'
# SQS
echo 'Starting SQS => Cases'
while ! awslocal sqs list-queues | grep mayamunka-test-queue
do
    awslocal --region=us-east-1 sqs create-queue --queue-name mayamunka-test-queue
done