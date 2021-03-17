# -*- coding: utf-8 -*-
"""
Spyder Editor

This is a temporary script file.
"""
import ast
import numpy as np
import matplotlib.pyplot as plt
import pandas as pd
import pymongo 
import json
import keras
import keras.backend as K
import sklearn.metrics as metrics
import gensim
from keras.models import Sequential
from keras.layers import Dense
from sklearn.preprocessing import LabelEncoder, OneHotEncoder, StandardScaler, MinMaxScaler
from sklearn.compose import ColumnTransformer
from sklearn.model_selection import train_test_split
from operator import itemgetter
from matplotlib import pyplot as plt
from keras.wrappers.scikit_learn import KerasRegressor, KerasClassifier
from sklearn.model_selection import cross_val_score
from gensim.models.doc2vec import Doc2Vec
from sklearn.model_selection import KFold

def build_regressor(input_size):
    
    #input_size = 238
    hl_1_dim = int(input_size / 2)
    hl_2_dim = int(hl_1_dim / 2)
    
    regressor = Sequential()
    regressor.add(Dense(units = hl_1_dim, kernel_initializer = 'normal', activation='relu', input_dim = input_size))
    regressor.add(Dense(units = hl_2_dim, kernel_initializer = 'normal', activation='relu'))
    regressor.add(Dense(units = 1, kernel_initializer = 'normal') )
    #optimizer = keras.optimizers.RMSprop()
    regressor.compile(optimizer = 'adam', loss = 'mean_squared_error')
    return regressor

def soft_acc(y_true, y_pred):
    return K.mean(K.equal(K.round(y_true), K.round(y_pred)))

def addWordColumnsAndData(articles, articleWordOccurenceData):
   
    newlist = sorted(articles, key=itemgetter('_id'))
    k = 0
    prevId = -1
    
    for item in articleWordOccurenceData:
        columnName ='Word.' + item['Word'];
        value = item['Occurence']
        articleId = item['ArticleId']
        
        newlist[k][columnName] = value
        
        if (prevId != articleId):
            if (prevId == -1):
                prevId = articleId
            else:
                prevId = articleId
                k += 1
           
             
    return newlist

def addTitleModelProcessedData(articles):
    model = Doc2Vec.load('D:\\wamp\\tmp\\titleModel.mod')
    for i, article in enumerate(articles):
        tokens = gensim.utils.simple_preprocess(article['Title'])
        vector = model.infer_vector(tokens)
        for l, num in enumerate(vector):
            articles[i]['Title.' + str(l)] = num
    return articles
        
def addArticleTextModelProcessedData(articles):
    model = Doc2Vec.load('D:\\wamp\\tmp\\articleTextModel.mod')
    for i, article in enumerate(articles):
        tokens = gensim.utils.simple_preprocess(article['Text'])
        vector = model.infer_vector(tokens)
        for l, num in enumerate(vector):
            articles[i]['Text.' + str(l)] = num
    return articles

def addTagData(articles):
    uniqueTags = getUniqueTags(articles)
    uniqueTags = ['Tags.' + str(x) for x in uniqueTags]
    
    for i, article in enumerate(articles):
        tags = article['Tags']
        for tag in tags:
            colName = 'Tags.' + str(tag['_id'])
            articles[i][colName] = 1.0
    return articles
        

def getUniqueTags(data):
    seen = set()
    unique = []
    for item in data:
        d = [o['_id'] for o in item.get('Tags')]
        for index in d:
            if index not in seen:
                seen.add(index)
                unique.append(index)
    unique.sort()    
    return unique
    

def getMongoClient():
    return pymongo.MongoClient("mongodb://admin:Pa$$w0rd@localhost:27017/")
    
def mongoGetArticles():
    mongoClient = getMongoClient()
    dbArticles = mongoClient["articleDataFinal"]
    articleCollection = dbArticles["articles"]
    return list(articleCollection.find({}))

def mongoGetArticleWordOccurence():
    mongoClient = getMongoClient()
    dbWordData = mongoClient["wordData"]
    articleWordOccurenceCollection = dbWordData["articleWordOccurence100Words"]
    return list(articleWordOccurenceCollection.find({}))


def GetNonVideoFocusedArticles(articles):
    return [article for article in articles if article['VideoFocusedArticle'] == False]

def addPopularColumn(articles):
    for i, article in enumerate(articles):
        if (article['Views'] > 2000): #3850
            articles[i]['Popular'] = 1
        else:
            articles[i]['Popular'] = 0
    return articles

articles = mongoGetArticles()
articleWordOccurenceData = mongoGetArticleWordOccurence()


#articles = GetNonVideoFocusedArticles(articles)

#articles = addPopularColumn(articles)

#articles = addWordColumnsAndData(articles, articleWordOccurenceData)

articles = addTitleModelProcessedData(articles)

articles = addArticleTextModelProcessedData(articles)

articles = addTagData(articles)


#aaa = [article for article in articles if article['ActivePeriod'] < 2772887]
#articles = aaa


df = pd.DataFrame(articles)

#df = df[31:].fillna(0) #tags NAN values to 0 # 116 for no titles
df = df.fillna(0)


df['Author'] = np.where((df.Author == 0), 'უცნობი', df.Author)
df['VideoFocusedArticle'] = np.where((df.VideoFocusedArticle == True), 1, 0)
df['BannerInsideArticle'] = np.where((df.BannerInsideArticle == True), 1, 0)
df['PublishedOnSocialMedia'] = np.where((df.PublishedOnSocialMedia == True), 1, 0)
df['BoostedOnSocialMedia'] = np.where((df.BoostedOnSocialMedia == True), 1, 0)

df = df.drop('Tags',1)
df = df.drop('Text',1)
df = df.drop('Title',1)
df = df.drop('Author',1)


#df = df.drop('Tags.161',1)
#df = df.drop('Tags.138',1)
#df = df.drop('Tags.140',1)
#df = df.drop('Tags.164',1)
#df = df.drop('Tags.162',1)


#ct = ColumnTransformer([("Author", OneHotEncoder(), [8])], remainder = 'passthrough')
#npArray  = ct.fit_transform(df)



npArray = np.array(df)
#npArray = sc.fit_transform(npArray)

X = np.delete(npArray, [0,2], 1) 
Y = npArray[:,2]



scX = MinMaxScaler()
scY = MinMaxScaler()

X = scX.fit_transform(X)
Y = scY.fit_transform(Y.reshape(-1,1))

X_train, X_test, Y_train, Y_test = train_test_split(X, Y, test_size = 0.3, shuffle = True) #TRIBUTE: Hitchhiker's Guide to the Galaxy 

regressor = build_regressor(len(X_train[0]))
regressor.fit(X_train, Y_train, batch_size = 5, epochs = 250)

#regressor = keras.models.load_model('D:\\wamp\\tmp\\MLClassifierModel_24_02_2021.mod')

#regressor.save('D:\\wamp\\tmp\\MLClassifierModel_24_02_2021.mod')

Y_pred = regressor.predict(X_test)

Y_pred = scY.inverse_transform(Y_pred)
Y_test = scY.inverse_transform(Y_test)


#f = plt.figure() 
#f.set_figwidth(1800) 
#f.set_figheight(1800) 

res = np.arange(1,len(Y_test)+1)


#plt.scatter(res, Y_test)
#plt.scatter(res, Y_pred)


#plt.plot(Y_pred)



X = res
Y = Y_test
total_bins = 300

bins = np.linspace(X.min(),X.max(), total_bins)
delta = bins[1]-bins[0]
idx  = np.digitize(X,bins)
running_median = [np.median(Y[idx==k]) for k in range(total_bins)]

plt.figure(figsize=(160, 160), dpi=80)
plt.scatter(X,Y,color='r',alpha=.2,s=2)
plt.plot(bins-delta/2,running_median,'r--',lw=1,alpha=.8)
plt.axis('tight')



X = res
Y = Y_pred


bins = np.linspace(X.min(),X.max(), total_bins)
delta = bins[1]-bins[0]
idx  = np.digitize(X,bins)
running_median = [np.median(Y[idx==k]) for k in range(total_bins)]


plt.scatter(X,Y,color='b',alpha=.2,s=2)
plt.plot(bins-delta/2,running_median,'b--',lw=1,alpha=.8)
plt.axis('tight')



regressor.save('D:\\wamp\\tmp\\MLRegressorModel_27_02_2021.mod')


# plt.gca().set_aspect('auto')
# res = np.arange(1,5826)
# plt.plot((np.array(Y_test)).flatten())

# plt.style.use('seaborn-whitegrid')

scores_regr = metrics.mean_absolute_error(Y_test, Y_pred)
kk = np.sqrt(scores_regr)


#score, acc = regressor.evaluate(X_test, Y_test,
#                            batch_size=10)
#print('Test score:', score)
#print('Test accuracy:', acc)



#estimator = KerasRegressor(build_fn=build_regressor, epochs=50, batch_size=5, verbose=10)
#kfold = KFold(n_splits=10)
#results = cross_val_score(estimator, X, Y, cv=10, error_score="raise")
#print("Baseline: %.2f (%.2f) MSE" % (results.mean(), results.std()))

