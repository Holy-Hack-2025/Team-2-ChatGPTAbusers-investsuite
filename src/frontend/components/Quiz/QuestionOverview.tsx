import LineGraph from '@components/graphs/LineGraph';

type Props = {
    question: any;
    handleAnswer: (answer: string) => void;
};

const QuestionOverview: React.FC<Props> = ({ question, handleAnswer }: Props) => {
    if (!question || !question.options) {
        return <p className="text-red-500">No data available.</p>;
    }

    console.log("Rendering QuestionOverview with:", question);

    return (
        <div className="flex flex-col w-full">
            <h3 className="text-black text-center">What is the graph of '{question.token}'</h3>
            <div className="text-black grid grid-cols-2 gap-3 p-3">
                {question.options.map((option: any, index: number) => (
                    <button
                        key={index}
                        className="w-full bg-gray-200 rounded p-4 cursor-pointer flex flex-col items-center gap-2"
                        onClick={() => handleAnswer(option.token)}
                    >
                        <span className="text-lg font-semibold">{option.token}</span>

                        {option.historicPrices ? (
                            <LineGraph 
                                lineGraphOptions={{
                                    title: option.token,
                                    yTitle: 'Value',
                                    xTitle: 'Last 30 days',
                                    start: 0,
                                    series: [{data:option.historicPrices, name: option.token}]
                                }}
                            />
                        ) : (
                            <p className="text-red-500">Invalid graph data</p>
                        )}
                    </button>
                ))}
            </div>
        </div>
    );
};

export default QuestionOverview;
